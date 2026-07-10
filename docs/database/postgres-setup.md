# Configuração do PostgreSQL

Este documento descreve como subir o PostgreSQL local, aplicar as migrations do
Entity Framework Core e rodar o backend (`Game.Api`).

## Pré-requisitos

* Docker instalado
* .NET SDK 8.0 instalado
* EF Core CLI instalado globalmente:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 1. Subir o PostgreSQL

Na raiz do repositório:

```bash
docker compose up -d
```

Isso cria o container `game-postgres` com:

* Database: `game_db`
* Usuário: `game_user`
* Senha: `game_password`
* Porta: `5432`

## 2. Aplicar as migrations

A partir da raiz do repositório:

```bash
dotnet ef database update --project src/Game.Infrastructure --startup-project src/Game.Api
```

Isso cria as tabelas iniciais no banco: `game_sessions`, `game_players`,
`questions`, `question_options`, `game_answers` e `rewards`.

## 3. Rodar o backend

```bash
cd src/Game.Api
dotnet run
```

Ao iniciar, a aplicação tenta popular a tabela `questions` com o conjunto de
perguntas padrão (seed idempotente — só insere se a tabela estiver vazia). Se o
banco ainda não estiver acessível, o jogo continua funcionando normalmente,
pois as regras e o estado da partida ainda usam o repositório em memória nesta
etapa da migração.

## Connection string usada localmente

Configurada em `src/Game.Api/appsettings.json`:

```
Host=localhost;Port=5432;Database=game_db;Username=game_user;Password=game_password
```

## Resetar o banco local

```bash
docker compose down -v
docker compose up -d
dotnet ef database update --project src/Game.Infrastructure --startup-project src/Game.Api
```

## Criar novas migrations

Sempre que uma entidade de persistência (`src/Game.Infrastructure/Persistence/Entities`)
for alterada, gere uma nova migration:

```bash
dotnet ef migrations add NomeDaMigration --project src/Game.Infrastructure --startup-project src/Game.Api
```

## Observação sobre o escopo atual

Nesta etapa, o PostgreSQL e o EF Core foram configurados e a migration inicial
(`InitialPostgresSetup`) já cria a estrutura de tabelas pensada para o core do
jogo (sessões, jogadores, perguntas, respostas e prêmios). O `GameService`
ainda usa os repositórios em memória (`InMemoryGameSessionRepository`,
`InMemoryQuestionRepository`) para não alterar as regras de negócio nesta
tarefa. A migração efetiva da persistência do core do jogo para o PostgreSQL
fica para uma etapa seguinte.
