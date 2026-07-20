namespace Game.Infrastructure.Persistence.Entities;

// Evento de auditoria do ciclo de vida de uma partida (append-only).
// Respostas e prêmios têm tabelas próprias (game_answers, rewards) — não viram eventos.
public class GameEventEntity
{
    public long Id { get; set; }
    public Guid GameSessionId { get; set; }

    // Jogador que causou o evento, quando aplicável.
    public Guid? PlayerId { get; set; }

    // Identidade opcional (D4): preenchida pela US8 quando houver usuário logado,
    // para o épico de Analytics filtrar por canal sem nunca somar as trilhas.
    public Guid? UserId { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }

    public GameSessionEntity? GameSession { get; set; }
}
