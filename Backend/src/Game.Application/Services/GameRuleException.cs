namespace Game.Application.Services;

// Erro de regra/fluxo do jogo (código inválido, sala cheia, fora da vez).
// O controller converte em resposta 4xx com a mensagem.
public class GameRuleException : Exception
{
    public GameRuleException(string message) : base(message)
    {
    }
}
