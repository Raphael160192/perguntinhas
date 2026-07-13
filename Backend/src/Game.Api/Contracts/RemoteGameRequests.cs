namespace Game.Api.Contracts;

public class CreateRemoteGameRequest
{
    public string Player1Name { get; set; } = "Jogador 1";
}

public class JoinGameRequest
{
    public string JoinCode { get; set; } = string.Empty;
    public string PlayerName { get; set; } = "Jogador 2";
}

public class NextRoundRequest
{
    public Guid? PlayerId { get; set; }
}
