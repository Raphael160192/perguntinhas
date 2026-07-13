# Plano: persistência completa no PostgreSQL + logs e auditoria

> **Status:** Fases 1 e 2 implementadas (migration `PersistGameSessions` +
> `PostgresGameSessionRepository`). Fases 3 e 4 pendentes.

## Contexto

Hoje o sistema está no ar com frontend na **Vercel** e backend + PostgreSQL na **Render**,
mas apenas o **catálogo de perguntas** vive no banco (`PostgresQuestionRepository`).
Todo o resto — sessões de jogo, jogadores, respostas, prêmios — fica em memória
(`InMemoryGameSessionRepository`), o que significa que:

1. **Toda partida em andamento é perdida** quando a Render reinicia ou faz deploy
   (no free tier o serviço hiberna após inatividade — as salas remotas morrem junto).
2. Não há **histórico**: nada de respostas dadas, prêmios sorteados, partidas jogadas.
3. Não há **logs estruturados** nem trilha de auditoria para análise futura
   (quantos jogos por dia, taxa de acerto por pergunta, temas mais difíceis etc.).

O objetivo deste plano é migrar o estado do jogo para o PostgreSQL e adicionar
persistência de logs/eventos, mantendo o monólito modular atual e **sem alterar as
regras do jogo nem os contratos da API consumidos pelo frontend**.

## Estado atual relevante (código)

| Item | Onde está | Situação |
|---|---|---|
| Sessões de jogo | `InMemoryGameSessionRepository` | memória (perde tudo no restart) |
| Perguntas | `PostgresQuestionRepository` + `QuestionSeeder` | já no banco ✅ |
| Entidades de persistência do jogo | `Game.Infrastructure/Persistence/Entities/*` | criadas na migration inicial, mas **desatualizadas** (não têm `Mode`, `JoinCode`, `QuestionOrder`) e sem uso |
| Respostas (`GameAnswerEntity`) | tabela existe | nunca gravada |
| Prêmios (`RewardEntity`) | tabela existe | nunca gravada, sem FK de sessão/jogador |
| Logs | `ILogger` padrão no console | sem estrutura, sem persistência |
| Migrations no deploy | `Database.MigrateAsync()` no startup (`Program.cs`) | já roda na Render ✅ |

## Tecnologias .NET

- **EF Core + Npgsql** (já no projeto) — persistência das sessões, respostas, prêmios e eventos.
- **Serilog** (`Serilog.AspNetCore`) — logging estruturado em JSON no console
  (a Render captura stdout no log stream; não é preciso sink pago).
- **Tabela `game_events` (jsonb)** — auditoria de negócio consultável via SQL,
  gravada pela própria aplicação com EF (sem dependência nova).
- **`IHostedService`** — job de limpeza/retenção (fase opcional).

Decisão importante: **logs de infraestrutura** (HTTP, exceções, startup) vão para o
console via Serilog (Render log stream); **eventos de negócio** (partida criada,
resposta dada, peça perdida, fim de jogo) vão para o banco na tabela `game_events`.
Persistir todo log HTTP no Postgres free tier (1 GB) esgotaria o espaço rapidamente.

---

## Fase 1 — Alinhar entidades de persistência ao domínio atual

As entidades em `Game.Infrastructure/Persistence/Entities` foram modeladas antes do
modo remoto. Atualizar:

**`GameSessionEntity`** (tabela `game_sessions`):
- adicionar `Mode` (string, ex: "Local"/"Remote"), `JoinCode` (string?, índice único
  filtrado para não-nulos), `QuestionOrderJson` (jsonb com a lista de **IDs** de
  perguntas — ver nota abaixo), `UpdatedAt`.

**`GamePlayerEntity`** (tabela `game_players`): já compatível (Name, Score, Socks,
Shirt, Pants, Underwear, ClothingLostAtScoresJson) — adicionar `PlayerIndex` (0/1)
para preservar a ordem dos jogadores.

**`GameAnswerEntity`** / **`RewardEntity`**: adicionar em `rewards` as colunas
`GameSessionId` (FK) e `PlayerId` para saber quem ganhou cada prêmio.

**Nota — `QuestionOrder` por ID, não por índice:** hoje `GameSession.QuestionOrder`
guarda *posições* na lista retornada por `GetAll()`. Com perguntas no banco, se uma
pergunta for adicionada/desativada no meio de uma partida os índices quebram.
Migrar para lista de `QuestionId` e trocar `GetCurrentQuestionEntity` para buscar
por ID (`IQuestionRepository.GetByIds(...)` ou dicionário). Ajuste pequeno e torna
a sessão estável para persistência.

**Migration:** `dotnet ef migrations add PersistGameSessions` — roda sozinha no
deploy da Render (startup já chama `MigrateAsync`).

Arquivos: `Persistence/Entities/*.cs`, `Persistence/Configurations/*.cs`,
`Game.Domain/Entities/GameSession.cs` (QuestionOrder por ID), `GameService.ShuffleQuestionOrder`.

## Fase 2 — Repositório de sessões no PostgreSQL

Criar `PostgresGameSessionRepository : IGameSessionRepository` em
`Game.Infrastructure/Repositories`, com mapeamento manual domínio ↔ entidade
(mesmo padrão do `PostgresQuestionRepository`):

- `Add` → insere `game_sessions` + `game_players`.
- `Get` / `GetByJoinCode` → `Include(Players)`, ordenando por `PlayerIndex`.
  `GetByJoinCode` filtra também `Status == WaitingForOpponent` para reaproveitar códigos antigos.
- `Update` → atualiza sessão + jogadores (score, roupas, `ClothingLostAtScoresJson`).

Trocar o registro no `Program.cs`:

```csharp
builder.Services.AddScoped<IGameSessionRepository, PostgresGameSessionRepository>();
```

(`Scoped`, não `Singleton`, porque depende do `ApplicationDbContext`.)

**Async:** converter `IGameSessionRepository`, `IQuestionRepository` e `IGameService`
para métodos `async Task<...>` (EF é I/O). Os controllers já são compatíveis
(vários já são `async` por causa dos broadcasts SignalR). Refactor mecânico.

**Concorrência (modo remoto):** dois aparelhos podem agir "ao mesmo tempo". Adicionar
concorrência otimista com a coluna de sistema `xmin` do Postgres
(`.Property(...).IsRowVersion()` no Npgsql); capturar `DbUpdateConcurrencyException`
e devolver `GameRuleException("Ação simultânea — tente de novo.")`. A validação
`EnsureIsCurrentPlayer` existente já elimina a maior parte dos conflitos.

**Manter o modo dev simples:** o docker-compose local já sobe o Postgres; o
`InMemoryGameSessionRepository` pode ser removido (ou mantido apenas em testes).

## Fase 3 — Persistir respostas e prêmios

No `GameService.SubmitAnswer` (via interfaces novas `IGameAnswerLog`/`IRewardLog` ou
direto no repositório de sessão para não inflar a arquitetura):

- **`game_answers`**: gravar a cada resposta — `GameSessionId`, `PlayerId`,
  `QuestionId` (agora estável), `SelectedOptionIndex`, `IsCorrect`, `CreatedAt`.
- **`rewards`**: gravar quando houver acerto — `GameSessionId`, `PlayerId` (quem
  ganhou), `Action`, `Location`, `TimeInSeconds`, `Text`, `CreatedAt`.

Isso habilita métricas imediatas por SQL: taxa de acerto por pergunta/tema,
duração média de partida, prêmios mais sorteados.

## Fase 4 — Logs estruturados + eventos de auditoria

**Serilog (infra):**
- Pacotes: `Serilog.AspNetCore` (+ `Serilog.Sinks.Console`).
- `Program.cs`: `builder.Host.UseSerilog(...)` com formato JSON compacto e
  `app.UseSerilogRequestLogging()` (método, rota, status, duração de cada request).
- Render mostra e retém esses logs no log stream do serviço.

**`game_events` (negócio):** nova tabela + entidade `GameEventEntity`:

| Coluna | Tipo | Exemplo |
|---|---|---|
| `Id` | bigserial | |
| `GameSessionId` | uuid (FK, index) | |
| `PlayerId` | uuid? | quem causou o evento |
| `EventType` | varchar(40) | `GameCreated`, `PlayerJoined`, `AnswerSubmitted`, `ClothingLost`, `RewardGranted`, `RoundAdvanced`, `GameFinished`, `GameRestarted` |
| `PayloadJson` | jsonb | detalhes específicos do evento |
| `CreatedAt` | timestamptz | |

Gravar via um `IGameEventLogger` (Application) implementado na Infrastructure,
chamado pelo `GameService` nos pontos de mutação. Um insert por evento na mesma
`SaveChanges` da operação (sem custo extra de round-trip).

**O que NÃO fazer agora:** sink do Serilog para Postgres (duplicaria os eventos e
incharia o banco), ELK/Seq/Application Insights (infra além do necessário).

## Fase 5 (futura, fora deste escopo) — Usuários e retenção

- **Usuários/contas**: hoje "usuário" = jogador dentro de uma sessão (sem login).
  Um cadastro real (e-mail/senha ou social) é um projeto próprio — criar tabela
  `users`, vincular `game_players.UserId` opcional, autenticação JWT. Deixar para
  quando houver funcionalidade que dependa disso (histórico pessoal, ranking).
- **Retenção/limpeza**: `IHostedService` diário que apaga sessões `WaitingForOpponent`
  com mais de 24h e eventos com mais de 90 dias (limite de 1 GB do Postgres free).

---

## Ordem de execução e entregas

1. Fase 1 + 2 juntas (uma migration, swap do repositório) — **valor imediato: salas sobrevivem a restart da Render**.
2. Fase 3 (respostas + prêmios) — pequena, encaixa no mesmo PR ou no seguinte.
3. Fase 4 (Serilog + game_events) — independente, pode ser paralela.

## Verificação

1. **Local**: `docker compose up -d` + `dotnet run`; criar partida local e remota,
   responder, verificar no banco (`game_sessions`, `game_players`, `game_answers`,
   `rewards`, `game_events`) via `psql`/DBeaver.
2. **Restart resiliente**: com uma partida remota em andamento, derrubar e subir a
   API; os dois aparelhos devem ressincronizar via `GET /api/games/{id}` (o
   frontend já faz isso no `onReconnected` do hub) e a partida continuar.
3. **Concorrência**: enviar duas respostas simultâneas (curl paralelo) e conferir
   que uma recebe erro de regra e o estado não corrompe.
4. **Frontend intocado**: jogar uma partida completa nos dois modos (local/remoto)
   na Vercel apontando para a Render — nenhum contrato de API mudou.
5. **Logs**: conferir o log stream da Render (JSON por request) e `SELECT * FROM
   game_events ORDER BY created_at DESC` após uma partida.
