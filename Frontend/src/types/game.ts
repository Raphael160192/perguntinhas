// Tipos espelhando os DTOs retornados pela API .NET (Game.Application.Dtos).
// O frontend não recalcula nenhuma regra: apenas reflete o que o backend retorna.

export interface ClothingState {
  socks: boolean;
  shirt: boolean;
  pants: boolean;
  underwear: boolean;
}

export interface Player {
  id: string;
  name: string;
  score: number;
  clothes: ClothingState;
  remainingClothesCount: number;
}

export interface Question {
  id: number;
  text: string;
  options: string[];
  level: number;
  theme: string;
  // Nunca contém o índice da resposta correta - isso fica só no backend.
}

export interface GameState {
  gameId: string;
  status: "InProgress" | "Finished";
  currentPlayerIndex: number;
  players: Player[];
  currentQuestion: Question | null;
  winnerPlayerId: string | null;
  winnerName: string | null;
  createdAt: string;
  finishedAt: string | null;
}

export interface AnswerResult {
  isCorrect: boolean;
  currentPlayer: Player;
  punishedPlayer: Player;
  lostClothing: string | null;
  reward: string | null;
  isGameOver: boolean;
  winner: Player | null;
  message: string;
  state: GameState;
}

export interface CreateGameResult {
  gameId: string;
  state: GameState;
}
