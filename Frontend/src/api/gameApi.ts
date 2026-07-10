// Concentra todas as chamadas HTTP ao backend .NET.
// Nenhum componente deve montar URLs ou fazer fetch diretamente.

import type { AnswerResult, CreateGameResult, GameState, Question } from "../types/game";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:7080";
const GAMES_URL = `${API_BASE_URL}/api/games`;

async function request<T>(path: string, method: "GET" | "POST", body?: unknown): Promise<T> {
  const response = await fetch(`${GAMES_URL}${path}`, {
    method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : undefined,
    body: body !== undefined ? JSON.stringify(body) : undefined
  });

  if (!response.ok) {
    throw new Error(`Falha ao comunicar com a API (status ${response.status})`);
  }

  return response.json() as Promise<T>;
}

export function createGame(player1Name: string, player2Name: string): Promise<CreateGameResult> {
  return request<CreateGameResult>("", "POST", { player1Name, player2Name });
}

export function getGame(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}`, "GET");
}

export function getCurrentQuestion(gameId: string): Promise<Question> {
  return request<Question>(`/${gameId}/question`, "GET");
}

export function answerQuestion(gameId: string, selectedOptionIndex: number): Promise<AnswerResult> {
  return request<AnswerResult>(`/${gameId}/answer`, "POST", { selectedOptionIndex });
}

export function nextRound(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}/next`, "POST");
}

export function restartGame(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}/restart`, "POST");
}
