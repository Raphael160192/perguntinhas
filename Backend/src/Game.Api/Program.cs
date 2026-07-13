using Game.Api.Hubs;
using Game.Application.Repositories;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Seed;
using Game.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// SignalR cross-origin exige AllowCredentials, que não pode ser combinado com AllowAnyOrigin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Nesta etapa, apenas o catálogo de perguntas vem do PostgreSQL.
// Sessões de jogo, jogadores, respostas e recompensas continuam em memória.
builder.Services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
builder.Services.AddScoped<IQuestionRepository, PostgresQuestionRepository>();
builder.Services.AddSingleton<IRewardProvider, RandomRewardProvider>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// O seed do PostgreSQL popula apenas perguntas/opções.
// Se o banco não estiver disponível, a API até inicia, mas criar partidas falhará
// até que o PostgreSQL esteja acessível.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await QuestionSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Não foi possível popular as perguntas no PostgreSQL no startup.");
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
