using Game.Api.Hubs;
using Game.Application.Repositories;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Seed;
using Game.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

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

// Nesta etapa, apenas o catálogo de perguntas vem do PostgreSQL.
// Sessões de jogo, jogadores, respostas e recompensas continuam em memória.
builder.Services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
builder.Services.AddScoped<IQuestionRepository, PostgresQuestionRepository>();
builder.Services.AddSingleton<IRewardProvider, RandomRewardProvider>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

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

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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

    if (uri.Scheme == Uri.UriSchemeHttp &&
        (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    return uri.Scheme == Uri.UriSchemeHttps &&
        uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
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
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ConnectionString;
}
