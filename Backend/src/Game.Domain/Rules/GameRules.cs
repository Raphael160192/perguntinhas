using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Domain.Rules;

public static class GameRules
{
    public static readonly ClothingItem[] ClothingOrder =
    {
        ClothingItem.Socks,
        ClothingItem.Shirt,
        ClothingItem.Pants,
        ClothingItem.Underwear
    };

    public static readonly int[] ClothingLossScores = { 9, 6, 3, 0 };

    public static AnswerOutcome ApplyAnswer(GameSession session, bool isCorrect)
    {
        var currentPlayer = session.CurrentPlayer;
        var opponentPlayer = session.OpponentPlayer;

        var punishedPlayer = isCorrect ? opponentPlayer : currentPlayer;

        punishedPlayer.Score--;
        if (punishedPlayer.Score < 0)
        {
            punishedPlayer.Score = 0;
        }

        ClothingItem? lostClothing = ApplyClothingLossIfNeeded(punishedPlayer);

        bool isGameOver = punishedPlayer.Clothes.RemainingCount() == 0;
        Player? winner = null;

        if (isGameOver)
        {
            winner = session.Players.First(p => p.Id != punishedPlayer.Id);
            session.Status = GameStatus.Finished;
            session.WinnerPlayerId = winner.Id;
            session.FinishedAt = DateTime.UtcNow;
        }

        return new AnswerOutcome
        {
            IsCorrect = isCorrect,
            CurrentPlayer = currentPlayer,
            PunishedPlayer = punishedPlayer,
            LostClothing = lostClothing,
            IsGameOver = isGameOver,
            Winner = winner
        };
    }

    private static ClothingItem? ApplyClothingLossIfNeeded(Player player)
    {
        bool shouldLoseClothing = ClothingLossScores.Contains(player.Score);
        bool alreadyLostAtThisScore = player.ClothingLostAtScores.Contains(player.Score);

        if (!shouldLoseClothing || alreadyLostAtThisScore)
        {
            return null;
        }

        var lost = LoseNextClothing(player);

        if (lost.HasValue)
        {
            player.ClothingLostAtScores.Add(player.Score);
        }

        return lost;
    }

    private static ClothingItem? LoseNextClothing(Player player)
    {
        foreach (var item in ClothingOrder)
        {
            if (player.Clothes.HasItem(item))
            {
                player.Clothes.LoseItem(item);
                return item;
            }
        }

        return null;
    }
}

public class AnswerOutcome
{
    public bool IsCorrect { get; set; }
    public Player CurrentPlayer { get; set; } = null!;
    public Player PunishedPlayer { get; set; } = null!;
    public ClothingItem? LostClothing { get; set; }
    public bool IsGameOver { get; set; }
    public Player? Winner { get; set; }
}
