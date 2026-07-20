import { useEffect, useRef, useState } from "react";
import {
  abandonGame,
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

function answerResultFromPending(state: GameState): AnswerResult | null {
  const pending = state.pendingRoundResult;
  if (!pending || pending.roundNumber !== state.roundNumber) return null;

  const currentPlayer = state.players.find((player) => player.id === pending.currentPlayerId);
  const punishedPlayer = state.players.find((player) => player.id === pending.punishedPlayerId);
  const winner = pending.winnerPlayerId
    ? state.players.find((player) => player.id === pending.winnerPlayerId) ?? null
    : null;

  if (!currentPlayer || !punishedPlayer) return null;

  return {
    isCorrect: pending.isCorrect,
    correctAnswerIndex: pending.correctAnswerIndex,
    currentPlayer,
    punishedPlayer,
    lostClothing: pending.lostClothing,
    reward: pending.rewardDetails?.text ?? pending.reward,
    rewardDetails: pending.rewardDetails,
    isGameOver: pending.isGameOver,
    winner,
    message: pending.isCorrect ? "Correto!" : "Errado!",
    state
  };
}

function hasTerminalPrize(result: AnswerResult | null): boolean {
  return Boolean(result?.isGameOver && result.isCorrect && (result.rewardDetails || result.reward));
}

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
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Aviso informativo exibido na home (ex: "Fulano encerrou a partida").
  const [notice, setNotice] = useState<string | null>(null);

  // Identidade deste aparelho no modo remoto (null = modo local).
  const [myPlayerId, setMyPlayerId] = useState<string | null>(null);
  const [joinCode, setJoinCode] = useState<string | null>(null);

  const isRemote = myPlayerId !== null;

  // Refs para os handlers do hub lerem valores atuais sem re-registrar a conexão.
  const gameIdRef = useRef<string | null>(null);
  gameIdRef.current = gameId;
  const myPlayerIdRef = useRef<string | null>(null);
  myPlayerIdRef.current = myPlayerId;

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

  // Recupera a partida salva no aparelho e deriva a tela do estado do servidor,
  // incluindo resultado ou prêmio pendente da rodada persistida.
  async function resumeSession(saved: SavedSession) {
    setLoading(true);
    setError(null);
    try {
      const state = await getGame(saved.gameId);

      // Partida encerrada por alguém: a sessão salva não vale mais.
      if (state.status === "Abandoned") {
        clearSession();
        setPhase("home");
        return;
      }

      setGameId(saved.gameId);
      setGameState(state);
      setMyPlayerId(saved.playerId);
      setJoinCode(state.joinCode ?? saved.joinCode);
      const pendingResult = answerResultFromPending(state);
      setAnswerResult(pendingResult);
      setSelectedOptionIndex(null);

      if (saved.playerId) {
        await connectToGame(saved.gameId, hubHandlers);
      }

      if (state.status === "WaitingForOpponent") {
        setPhase("remote-create");
      } else if (pendingResult && (state.status !== "Finished" || hasTerminalPrize(pendingResult))) {
        setPhase("result");
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
    setPhase("question");
  }

  function handleGameRestarted(state: GameState) {
    setGameState(state);
    setAnswerResult(null);
    setSelectedOptionIndex(null);
    setPhase("question");
  }

  // O outro jogador saiu: limpa tudo e avisa na home, durante ou após a partida.
  function handleGameAbandoned(payload: {
    state: GameState;
    abandonedByPlayerId: string | null;
    abandonedByName: string | null;
  }) {
    // O broadcast também chega a quem fez a chamada; esse aparelho já está saindo.
    if (
      payload.abandonedByPlayerId &&
      payload.abandonedByPlayerId === myPlayerIdRef.current
    ) return;

    abandonSession();
    setNotice(
      payload.abandonedByName
        ? `${payload.abandonedByName} saiu da partida.`
        : "O outro jogador saiu da partida."
    );
  }

  async function handleReconnected() {
    // Reconectou após queda: ressincroniza o estado pela API.
    const id = gameIdRef.current;
    if (!id) return;
    try {
      const state = await getGame(id);
      setGameState(state);
      const pendingResult = answerResultFromPending(state);
      if (pendingResult && (state.status !== "Finished" || hasTerminalPrize(pendingResult))) {
        setAnswerResult(pendingResult);
        setPhase("result");
      } else if (state.status === "Finished") {
        setAnswerResult(null);
        setPhase("gameover");
      } else {
        setAnswerResult(null);
        setSelectedOptionIndex(null);
        setPhase("question");
      }
    } catch {
      // mantém a tela atual; próxima ação do usuário mostra o erro
    }
  }

  const hubHandlers = {
    onPlayerJoined: handlePlayerJoined,
    onAnswerSubmitted: handleAnswerSubmitted,
    onRoundAdvanced: handleRoundAdvanced,
    onGameRestarted: handleGameRestarted,
    onGameAbandoned: handleGameAbandoned,
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
      const pendingResult = answerResultFromPending(joined.state);
      setAnswerResult(pendingResult);
      saveSession({
        gameId: joined.gameId,
        playerId: joined.playerId,
        joinCode: code,
        playerName
      });
      await connectToGame(joined.gameId, hubHandlers);
      setPhase(
        pendingResult && (joined.state.status !== "Finished" || hasTerminalPrize(pendingResult))
            ? "result"
          : joined.state.status === "Finished"
            ? "gameover"
            : "question"
      );
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

    if (answerResult.isCorrect && (answerResult.rewardDetails || answerResult.reward)) {
      setPhase("prize");
    } else if (answerResult.isGameOver) {
      setPhase("gameover");
    } else {
      void advanceRound();
    }
  }

  function handleFinishPrize() {
    if (answerResult?.isGameOver) {
      setPhase("gameover");
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

  // Encerra a partida no backend (para os dois aparelhos) e limpa o estado local.
  async function handleAbandonGame() {
    const saved = loadSession();
    const id = gameId ?? saved?.gameId;
    const player = myPlayerId ?? saved?.playerId ?? undefined;

    setLoading(true);
    try {
      if (id) {
        await abandonGame(id, player);
      }
    } catch {
      // Mesmo se a API falhar (sala já apagada etc.), sai localmente.
    } finally {
      setLoading(false);
      abandonSession();
    }
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
          notice={notice}
          onLocal={() => {
            setError(null);
            setNotice(null);
            setPhase("setup");
          }}
          onRemoteCreate={() => {
            setError(null);
            setNotice(null);
            setPhase("remote-create");
          }}
          onRemoteJoin={() => {
            setError(null);
            setNotice(null);
            setPhase("remote-join");
          }}
          resumeAvailable={savedForResume !== null}
          resumeJoinCode={savedForResume?.joinCode ?? null}
          onResume={() => {
            setNotice(null);
            const saved = loadSession();
            if (saved) void resumeSession(saved);
          }}
          onAbandon={() => void handleAbandonGame()}
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
          onCancelRoom={() => void handleAbandonGame()}
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
            selectedOptionIndex={selectedOptionIndex}
            loading={loading}
            interactive={isMyTurn}
            onSelect={setSelectedOptionIndex}
            onConfirm={handleConfirmAnswer}
            onRestart={handleRestart}
            onAbandon={() => void handleAbandonGame()}
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
          onDone={handleFinishPrize}
          onSkip={handleFinishPrize}
          loading={loading}
          showActions={answeredByMe || answerResult.isGameOver}
        />
      )}

      {phase === "gameover" && gameState && (
        <GameOverScreen
          state={gameState}
          onRestart={handleRestart}
          onExit={() => void handleAbandonGame()}
          loading={loading}
        />
      )}
    </div>
  );
}
