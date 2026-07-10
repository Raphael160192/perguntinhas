import { useEffect, useState } from "react";
import { answerQuestion, createGame, nextRound, restartGame } from "./api/gameApi";
import { GameHeader } from "./components/GameHeader";
import { QuestionCard } from "./components/QuestionCard";
import { GameResult } from "./components/GameResult";
import { GameOverModal } from "./components/GameOverModal";
import type { AnswerResult, GameState } from "./types/game";

export default function App() {
  const [gameId, setGameId] = useState<string | null>(null);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [selectedOptionIndex, setSelectedOptionIndex] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isGameOverModalOpen, setIsGameOverModalOpen] = useState(false);

  // Ao montar a aplicação, cria automaticamente uma nova partida no backend.
  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        const created = await createGame("Jogador 1", "Jogador 2");
        if (cancelled) return;
        setGameId(created.gameId);
        setGameState(created.state);
      } catch {
        if (!cancelled) setError("Não foi possível iniciar a partida. Verifique se a API está rodando.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleCheckAnswer() {
    if (selectedOptionIndex === null) {
      setError("Por favor, selecione uma opção.");
      return;
    }
    if (!gameId) return;

    setError(null);
    setLoading(true);

    try {
      const result = await answerQuestion(gameId, selectedOptionIndex);
      setAnswerResult(result);
      setGameState(result.state);

      if (result.isGameOver) {
        setIsGameOverModalOpen(true);
      }
    } catch {
      setError("Não foi possível enviar a resposta. Tente novamente.");
    } finally {
      setLoading(false);
    }
  }

  async function handleNextRound() {
    if (!gameId) return;

    setError(null);
    setLoading(true);

    try {
      const state = await nextRound(gameId);
      setGameState(state);
      setSelectedOptionIndex(null);
      setAnswerResult(null);
    } catch {
      setError("Não foi possível avançar para a próxima rodada.");
    } finally {
      setLoading(false);
    }
  }

  async function handleRestart() {
    if (!gameId) return;

    setError(null);
    setLoading(true);

    try {
      const state = await restartGame(gameId);
      setGameState(state);
      setSelectedOptionIndex(null);
      setAnswerResult(null);
      setIsGameOverModalOpen(false);
    } catch {
      setError("Não foi possível reiniciar a partida.");
    } finally {
      setLoading(false);
    }
  }

  if (!gameState) {
    return <div className="container content">{error ?? "Carregando partida..."}</div>;
  }

  const hasAnswered = answerResult !== null;
  const isGameFinished = gameState.status === "Finished";

  return (
    <div className="container">
      <GameHeader state={gameState} />

      <div className="content">
        {gameState.currentQuestion && (
          <QuestionCard
            question={gameState.currentQuestion}
            selectedOptionIndex={selectedOptionIndex}
            disabled={hasAnswered || isGameFinished}
            onSelect={setSelectedOptionIndex}
          />
        )}

        <GameResult result={answerResult} />

        {error && <div className="error">{error}</div>}

        <div className="actions">
          <button
            className="btn btn-success"
            onClick={handleCheckAnswer}
            disabled={loading || hasAnswered || isGameFinished}
          >
            Verificar
          </button>
          <button
            className="btn btn-primary"
            onClick={handleNextRound}
            disabled={loading || !hasAnswered || isGameFinished}
          >
            Próxima
          </button>
        </div>
      </div>

      <GameOverModal
        open={isGameOverModalOpen}
        winnerName={gameState.winnerName}
        onRestart={handleRestart}
        onClose={() => setIsGameOverModalOpen(false)}
      />
    </div>
  );
}
