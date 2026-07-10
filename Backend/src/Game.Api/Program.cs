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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// O core do jogo (GameSession em memória) ainda não foi migrado para o PostgreSQL nesta etapa.
// O DbContext acima já está pronto para uso, mas os repositórios abaixo continuam em memória
// para não alterar as regras de negócio existentes.
builder.Services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
builder.Services.AddSingleton<IQuestionRepository, InMemoryQuestionRepository>();
builder.Services.AddSingleton<IRewardProvider, RandomRewardProvider>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// O seed do PostgreSQL é best-effort: se o banco ainda não estiver disponível,
// o jogo continua funcionando normalmente com o repositório em memória.
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
        logger.LogWarning(ex, "Não foi possível popular o PostgreSQL no startup. O jogo continuará usando o repositório em memória.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
