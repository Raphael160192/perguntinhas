// Conexão SignalR para sincronizar as telas dos dois aparelhos em tempo real.
// O backend faz broadcast por grupo (gameId) após cada mutação da partida.

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from "@microsoft/signalr";
import { API_BASE_URL } from "./gameApi";
import type { AnswerResult, GameState } from "../types/game";

export interface GameHubHandlers {
  onPlayerJoined?: (state: GameState) => void;
  onAnswerSubmitted?: (result: AnswerResult) => void;
  onRoundAdvanced?: (state: GameState) => void;
  onGameRestarted?: (state: GameState) => void;
  onReconnected?: () => void;
}

let connection: HubConnection | null = null;

export async function connectToGame(gameId: string, handlers: GameHubHandlers): Promise<void> {
  await disconnectFromGame();

  connection = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/game`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  connection.on("PlayerJoined", (state: GameState) => handlers.onPlayerJoined?.(state));
  connection.on("AnswerSubmitted", (result: AnswerResult) => handlers.onAnswerSubmitted?.(result));
  connection.on("RoundAdvanced", (state: GameState) => handlers.onRoundAdvanced?.(state));
  connection.on("GameRestarted", (state: GameState) => handlers.onGameRestarted?.(state));

  // Ao reconectar, reentra no grupo e avisa o App para ressincronizar o estado.
  connection.onreconnected(async () => {
    await connection?.invoke("JoinGameGroup", gameId);
    handlers.onReconnected?.();
  });

  await connection.start();
  await connection.invoke("JoinGameGroup", gameId);
}

export async function disconnectFromGame(): Promise<void> {
  if (connection && connection.state !== HubConnectionState.Disconnected) {
    try {
      await connection.stop();
    } catch {
      // conexão já encerrada
    }
  }
  connection = null;
}
