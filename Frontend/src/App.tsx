import { useEffect, useRef, useState } from "react";
import {
  answerQuestion,
  createGame,
  createRemoteGame,
  getGame,
  joinGame,
  nextRound,
  restartGame
} from "./api/gameApi";
import { connectToGame, disconnectFromGame } from "./api/gameHub";
import { HomeScreen } from "./components/HomeScreen";
import { PlayerSetup } from "./components/PlayerSetup";
import { RemoteCreate } from "./components/RemoteCreate";
import { RemoteJoin } from "./components/RemoteJoin";
import { QuestionScreen } from "./components/QuestionScreen";
import { ResultTakeover } from "./components/ResultTakeover";
import { PrizeSheet } from "./components/PrizeSheet";
import { GameOverScreen } from "./components/GameOverScreen";
import type { AnswerResult, GameState } from "./types/game";

// Fases da interface. Toda regra do jogo vem da API — aqui só controlamos qual tela mostrar.
type Phase =
  | "home"
  | "setup"
  | "remote-create"
  | "remote-join"
  | "question"
  | "result"
  | "prize"
  | "gameover";

export default function App() {
  const [phase, setPhase] = useState<Phase>("home");
  const [gameId, setGameId] = useState<string | null>(null);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [selectedOptionIndex, setSelectedOptionIndex] = useState<number | null>(null);
  const [round, setRound] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Identidade deste aparelho no modo remoto (null = modo local).
  const [myPlayerId, setMyPlayerId] = useState<string | null>(null);
  const [joinCode, setJoinCode] = useState<string | null>(null);

  const isRemote = myPlayerId !== null;

  // Refs para os handlers do hub lerem valores atuais sem re-registrar a conexão.
  const gameIdRef = useRef<string | null>(null);
  gameIdRef.current = gameId;

  useEffect(() => {
    return () => {
      void disconnectFromGame();
    };
  }, []);

  // ===== Eventos vindos do outro aparelho (SignalR) =====

  function handlePlayerJoined(state: GameState) {
    setGameState(state);
    setRound(1);
    setPhase("question");
  }

  function handleAnswerSubmitted(result: AnswerResult) {
    setAnswerResult(result);
    setGameState(result.state);
    setPhase("result");
  }

  function handleRoundAdvanced(state: GameState) {
    setGameState(state);
    setAnswerResult(null);
    setSelectedOptionIndex(null);
    setRound((r) => r + 1);
    setPhase("question");
  }

  function handleGameRestarted(state: GameState) {
    setGameState(state);
    setAnswerResult(null);
    setSelectedOptionIndex(null);
    setRound(1);
    setPhase("question");
  }

  async function handleReconnected() {
    // Reconectou após queda: ressincroniza o estado pela API.
    const id = gameIdRef.current;
    if (!id) return;
    try {
      const state = await getGame(id);
      setGameState(state);
      if (state.status === "Finished") setPhase("gameover");
    } catch {
      // mantém a tela atual; próxima ação do usuário mostra o erro
    }
  }

  const hubHandlers = {
    onPlayerJoined: handlePlayerJoined,
    onAnswerSubmitted: handleAnswerSubmitted,
    onRoundAdvanced: handleRoundAdvanced,
    onGameRestarted: handleGameRestarted,
    onReconnected: handleReconnected
  };

  // ===== Ações do usuário =====

  async function handleStartLocal(player1Name: string, player2Name: string) {
    setError(null);
    setLoading(true);
    try {
      const created = await createGame(player1Name, player2Name);
      setGameId(created.gameId);
      setGameState(created.state);
      setMyPlayerId(null);
      setRound(1);
      setPhase("question");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível criar a partida.");
    } finally {
      setLoading(false);
    }
  }

  async function handleCreateRemote(playerName: string) {
    setError(null);
    setLoading(true);
    try {
      const created = await createRemoteGame(playerName);
      setGameId(created.gameId);
      setGameState(created.state);
      setMyPlayerId(created.playerId);
      setJoinCode(created.joinCode);
      await connectToGame(created.gameId, hubHandlers);
      // permanece na fase remote-create exibindo o código até o PlayerJoined chegar
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível criar a sala.");
    } finally {
      setLoading(false);
    }
  }

  async function handleJoinRemote(code: string, playerName: string) {
    setError(null);
    setLoading(true);
    try {
      const joined = await joinGame(code, playerName);
      setGameId(joined.gameId);
      setGameState(joined.state);
      setMyPlayerId(joined.playerId);
      await connectToGame(joined.gameId, hubHandlers);
      setRound(1);
      setPhase("question");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível entrar na sala.");
    } finally {
      setLoading(false);
    }
  }

  async function handleConfirmAnswer() {
    if (!gameId || selectedOptionIndex === null) return;

    setError(null);
    setLoading(true);
    try {
      const result = await answerQuestion(gameId, selectedOptionIndex, myPlayerId ?? undefined);
      setAnswerResult(result);
      setGameState(result.state);
      setPhase("result");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível enviar a resposta.");
    } finally {
      setLoading(false);
    }
  }

  // Sai do takeover: acerto vai para o prêmio; erro (ou fim de jogo) segue direto.
  function handleContinueFromResult() {
    if (!answerResult) return;

    if (answerResult.isGameOver) {
      setPhase("gameover");
      return;
    }

    if (answerResult.isCorrect && answerResult.reward) {
      setPhase("prize");
    } else {
      void advanceRound();
    }
  }

  async function advanceRound() {
    if (!gameId) return;

    setError(null);
    setLoading(true);
    try {
      const state = await nextRound(gameId, myPlayerId ?? undefined);
      setGameState(state);
      setAnswerResult(null);
      setSelectedOptionIndex(null);
      // No modo remoto o incremento vem do broadcast RoundAdvanced (que também chega a este aparelho).
      if (!isRemote) {
        setRound((r) => r + 1);
      }
      setPhase("question");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível avançar a rodada.");
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
      setAnswerResult(null);
      setSelectedOptionIndex(null);
      setRound(1);
      setPhase("question");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Não foi possível reiniciar a partida.");
    } finally {
      setLoading(false);
    }
  }

  function backToHome() {
    setError(null);
    setJoinCode(null);
    setPhase("home");
  }

  // No modo remoto, este aparelho só interage quando é a vez do seu jogador.
  const isMyTurn =
    !isRemote ||
    (gameState !== null && gameState.players[gameState.currentPlayerIndex]?.id === myPlayerId);

  // O espectador do resultado é quem NÃO respondeu a pergunta da rodada.
  const answeredByMe = !isRemote || answerResult?.currentPlayer.id === myPlayerId;

  return (
    <div className="app">
      {phase === "home" && (
        <HomeScreen
          onLocal={() => {
            setError(null);
            setPhase("setup");
          }}
          onRemoteCreate={() => {
            setError(null);
            setPhase("remote-create");
          }}
          onRemoteJoin={() => {
            setError(null);
            setPhase("remote-join");
          }}
        />
      )}

      {phase === "setup" && (
        <PlayerSetup onStart={handleStartLocal} loading={loading} error={error} />
      )}

      {phase === "remote-create" && (
        <RemoteCreate
          joinCode={joinCode}
          onCreate={handleCreateRemote}
          onBack={backToHome}
          loading={loading}
          error={error}
        />
      )}

      {phase === "remote-join" && (
        <RemoteJoin
          onJoin={handleJoinRemote}
          onBack={backToHome}
          loading={loading}
          error={error}
        />
      )}

      {phase === "question" && gameState && (
        <>
          <QuestionScreen
            state={gameState}
            round={round}
            selectedOptionIndex={selectedOptionIndex}
            loading={loading}
            interactive={isMyTurn}
            onSelect={setSelectedOptionIndex}
            onConfirm={handleConfirmAnswer}
          />
          {error && <div className="error-note">{error}</div>}
        </>
      )}

      {phase === "result" && answerResult && (
        <ResultTakeover
          result={answerResult}
          onContinue={handleContinueFromResult}
          loading={loading}
          // No fim de jogo a transição é local (sem broadcast), então os dois aparelhos
          // precisam do botão "Ver resultado".
          showActions={answeredByMe || answerResult.isGameOver}
        />
      )}

      {phase === "prize" && answerResult && (
        <PrizeSheet
          result={answerResult}
          onDone={advanceRound}
          onSkip={advanceRound}
          loading={loading}
          showActions={answeredByMe}
        />
      )}

      {phase === "gameover" && gameState && (
        <GameOverScreen state={gameState} onRestart={handleRestart} loading={loading} />
      )}
    </div>
  );
}
