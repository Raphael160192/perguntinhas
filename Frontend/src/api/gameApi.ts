// Concentra todas as chamadas HTTP ao backend .NET.
// Nenhum componente deve montar URLs ou fazer fetch diretamente.

import type {
  AnswerResult,
  CreateGameResult,
  CreateRemoteGameResult,
  GameState,
  JoinGameResult,
  Question
} from "../types/game";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5042";
const GAMES_URL = `${API_BASE_URL}/api/games`;

async function request<T>(path: string, method: "GET" | "POST", body?: unknown): Promise<T> {
  const response = await fetch(`${GAMES_URL}${path}`, {
    method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : undefined,
    body: body !== undefined ? JSON.stringify(body) : undefined
  });

  if (!response.ok) {
    // Erros de regra (400/409) vêm com { message } no corpo.
    let message = `Falha ao comunicar com a API (status ${response.status})`;
    try {
      const data = await response.json();
      if (data?.message) message = data.message;
    } catch {
      // corpo não-JSON; mantém a mensagem padrão
    }
    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

export function createGame(player1Name: string, player2Name: string): Promise<CreateGameResult> {
  return request<CreateGameResult>("", "POST", { player1Name, player2Name });
}

export function createRemoteGame(player1Name: string): Promise<CreateRemoteGameResult> {
  return request<CreateRemoteGameResult>("/remote", "POST", { player1Name });
}

export function joinGame(joinCode: string, playerName: string): Promise<JoinGameResult> {
  return request<JoinGameResult>("/join", "POST", { joinCode, playerName });
}

export function getGame(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}`, "GET");
}

export function getCurrentQuestion(gameId: string): Promise<Question> {
  return request<Question>(`/${gameId}/question`, "GET");
}

export function answerQuestion(
  gameId: string,
  selectedOptionIndex: number,
  playerId?: string
): Promise<AnswerResult> {
  return request<AnswerResult>(`/${gameId}/answer`, "POST", { selectedOptionIndex, playerId });
}

export function nextRound(gameId: string, playerId?: string): Promise<GameState> {
  return request<GameState>(`/${gameId}/next`, "POST", { playerId });
}

export function restartGame(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}/restart`, "POST");
}
