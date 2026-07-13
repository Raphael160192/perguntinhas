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
import { clearSession, loadSession, saveSession, type SavedSession } from "./api/session";
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
  | "resuming"
  | "home"
  | "setup"
  | "remote-create"
  | "remote-join"
  | "question"
  | "result"
  | "prize"
  | "gameover";

export default function App() {
  // Com sessão salva, o app abre em "retomando…" (refresh volta direto ao jogo,
  // sem piscar a home com o banner de retomada).
  const [phase, setPhase] = useState<Phase>(() => (loadSession() ? "resuming" : "home"));
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

  // Retomada automática: refresh acidental volta direto para a partida salva.
  useEffect(() => {
    const saved = loadSession();
    if (saved) {
      void resumeSession(saved);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Recupera a partida salva no aparelho e deriva a tela do estado do servidor.
  // Takeovers de resultado/prêmio são transitórios: a retomada cai na pergunta atual.
  async function resumeSession(saved: SavedSession) {
    setLoading(true);
    setError(null);
    try {
      const state = await getGame(saved.gameId);

      setGameId(saved.gameId);
      setGameState(state);
      setMyPlayerId(saved.playerId);
      setJoinCode(state.joinCode ?? saved.joinCode);
      setAnswerResult(null);
      setSelectedOptionIndex(null);

      if (saved.playerId) {
        await connectToGame(saved.gameId, hubHandlers);
      }

      if (state.status === "WaitingForOpponent") {
        setPhase("remote-create");
      } else if (state.status === "Finished") {
        setPhase("gameover");
      } else {
        setPhase("question");
      }
    } catch {
      // Sala não existe mais (expirada/apagada): descarta a sessão salva.
      clearSession();
      setPhase("home");
    } finally {
      setLoading(false);
    }
  }

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
      saveSession({ gameId: created.gameId, playerId: null, joinCode: null, playerName: null });
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
      saveSession({
        gameId: created.gameId,
        playerId: created.playerId,
        joinCode: created.joinCode,
        playerName
      });
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
      // Envia a identidade salva: se este aparelho já é jogador desta sala,
      // o backend devolve a sessão em andamento (rejoin) em vez de "sala cheia".
      const savedPlayerId = loadSession()?.playerId ?? undefined;
      const joined = await joinGame(code, playerName, savedPlayerId);
      setGameId(joined.gameId);
      setGameState(joined.state);
      setMyPlayerId(joined.playerId);
      saveSession({
        gameId: joined.gameId,
        playerId: joined.playerId,
        joinCode: code,
        playerName
      });
      await connectToGame(joined.gameId, hubHandlers);
      setRound(1);
      setPhase(joined.state.status === "Finished" ? "gameover" : "question");
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

  // Abandono explícito: descarta a sessão salva e zera o estado local.
  function abandonSession() {
    clearSession();
    void disconnectFromGame();
    setGameId(null);
    setGameState(null);
    setAnswerResult(null);
    setSelectedOptionIndex(null);
    setMyPlayerId(null);
    setJoinCode(null);
    setError(null);
    setPhase("home");
  }

  // Sessão salva ainda válida para oferecer "voltar à partida" na home.
  const savedForResume = phase === "home" ? loadSession() : null;

  // No modo remoto, este aparelho só interage quando é a vez do seu jogador.
  const isMyTurn =
    !isRemote ||
    (gameState !== null && gameState.players[gameState.currentPlayerIndex]?.id === myPlayerId);

  // O espectador do resultado é quem NÃO respondeu a pergunta da rodada.
  const answeredByMe = !isRemote || answerResult?.currentPlayer.id === myPlayerId;

  return (
    <div className="app">
      {phase === "resuming" && (
        <div className="screen screen--question">
          <div className="loading-screen">retomando a partida…</div>
        </div>
      )}

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
          resumeAvailable={savedForResume !== null}
          resumeJoinCode={savedForResume?.joinCode ?? null}
          onResume={() => {
            const saved = loadSession();
            if (saved) void resumeSession(saved);
          }}
          onAbandon={abandonSession}
          loading={loading}
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
        <GameOverScreen
          state={gameState}
          onRestart={handleRestart}
          onExit={abandonSession}
          loading={loading}
        />
      )}
    </div>
  );
}
