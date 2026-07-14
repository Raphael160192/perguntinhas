namespace Game.Application.Repositories;

// Catálogo dos eventos de auditoria do ciclo de vida da partida (tabela game_events).
public static class GameEventTypes
{
    public const string GameCreated = "GameCreated";
    public const string PlayerJoined = "PlayerJoined";
    public const string ClothingLost = "ClothingLost";
    public const string GameFinished = "GameFinished";
    public const string GameRestarted = "GameRestarted";
}
