using System.Text;
using Game.Api.Auth;
using Game.Api.Hubs;
using Game.Application.Auth;
using Game.Application.Repositories;
using Game.Application.Rewards;
using Game.Application.Services;
using Game.Infrastructure.Auth;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Seed;
using Game.Infrastructure.Repositories;
using Game.Infrastructure.Rewards;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados: JSON compacto por linha no console (capturado pelo log
// stream da Render). Níveis configuráveis pela seção "Serilog" do appsettings.
builder.Host.UseSerilog((context, config) =>
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
var allowedOriginSet = allowedOrigins
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.SetIsOriginAllowed(origin => IsAllowedFrontendOrigin(origin, allowedOriginSet))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(ToNpgsqlConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Persistência no PostgreSQL: perguntas e sessões de jogo (partidas sobrevivem a restarts).
builder.Services.AddScoped<IGameSessionRepository, PostgresGameSessionRepository>();
builder.Services.AddScoped<IQuestionRepository, PostgresQuestionRepository>();
builder.Services.AddScoped<IGameActivityLog, PostgresGameActivityLog>();
builder.Services.AddSingleton<IRewardProvider, RandomRewardProvider>();
builder.Services.Configure<RewardsOptions>(builder.Configuration.GetSection(RewardsOptions.SectionName));
builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<IRewardCatalog>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RewardsOptions>>().Value;
    return new JsonRewardCatalog(options.CatalogResource);
});
builder.Services.AddSingleton<RewardSelector>();
builder.Services.AddSingleton<LegacyRewardSelector>();
builder.Services.AddSingleton<IRewardSelector>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RewardsOptions>>().Value;
    return options.IntelligentSelectionEnabled
        ? serviceProvider.GetRequiredService<RewardSelector>()
        : serviceProvider.GetRequiredService<LegacyRewardSelector>();
});
builder.Services.AddScoped<IGameService, GameService>();

// Fatia-base de identidade (F0). A SigningKey vem de variável de ambiente
// (Jwt__SigningKey); Issuer/Audience/lifetime do appsettings.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantém o claim 'sub' cru (sem mapear para NameIdentifier).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrEmpty(jwtOptions.SigningKey)
                    ? new string('0', 32) // placeholder inócuo: sem token emitido, nada valida em F0
                    : jwtOptions.SigningKey))
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddSingleton<IJwtIssuer, JwtIssuer>();
builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
builder.Services.AddScoped<IUserConsentLog, PostgresUserConsentLog>();

// Login com Google (US6): valida o ID token e emite o JWT próprio.
builder.Services.Configure<GoogleAuthOptions>(builder.Configuration.GetSection(GoogleAuthOptions.SectionName));
builder.Services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Quando a seleção inteligente está ativa, carrega e valida o catálogo no
// startup. Assim, um catálogo inválido impede o deploy de receber partidas.
var rewardsOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RewardsOptions>>()
    .Value;
if (rewardsOptions.IntelligentSelectionEnabled)
{
    _ = app.Services.GetRequiredService<IRewardCatalog>();
}

// O catálogo de perguntas é obrigatório para criar partidas. No startup, a API
// aplica migrations pendentes e popula as perguntas iniciais se a tabela estiver vazia.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await QuestionSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Não foi possível preparar o catálogo de perguntas no PostgreSQL.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exceções não tratadas viram 500 JSON com headers de CORS — assim o navegador
// mostra o erro real em vez de mascarar como "CORS blocked". O UseExceptionHandler
// limpa a resposta (incluindo headers já adicionados pelo UseCors), então o header
// de origem é reaplicado manualmente aqui.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(feature?.Error, "Erro não tratado em {Path}.", context.Request.Path);

        var origin = context.Request.Headers.Origin.ToString();
        if (!string.IsNullOrEmpty(origin) && IsAllowedFrontendOrigin(origin, allowedOriginSet))
        {
            context.Response.Headers.AccessControlAllowOrigin = origin;
            context.Response.Headers.AccessControlAllowCredentials = "true";
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "Erro interno no servidor. Tente novamente." });
    });
});

// Uma linha estruturada por request (método, rota, status, duração).
app.UseSerilogRequestLogging();

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/hubs/game");

app.Run();

static bool IsAllowedFrontendOrigin(string origin, ISet<string> configuredOrigins)
{
    var normalizedOrigin = origin.Trim().TrimEnd('/');
    if (configuredOrigins.Contains(normalizedOrigin))
    {
        return true;
    }

    if (!Uri.TryCreate(normalizedOrigin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    // Apenas localhost em dev é liberado além dos origins configurados. Domínios de
    // produção (incl. o da Vercel) devem estar em Cors:AllowedOrigins — com JWT via
    // credenciais, o curinga *.vercel.app anterior era uma vulnerabilidade (task #18).
    return uri.Scheme == Uri.UriSchemeHttp &&
        (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase));
}

static string ToNpgsqlConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return string.Empty;
    }

    if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return connectionString;
    }

    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = SslMode.Require
    }.ConnectionString;
}
