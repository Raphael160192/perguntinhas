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

export interface RewardDetails {
  templateId: string;
  catalogVersion: string;
  text: string;
  level: 1 | 2 | 3 | 4;
  levelName: string;
  actorPlayerId: string;
  receiverPlayerId: string;
  executionType: "Seconds" | "Repetitions" | "FreeForm";
  executionValue: string;
}

export interface PendingRoundResult {
  roundNumber: number;
  correctAnswerIndex: number;
  isCorrect: boolean;
  currentPlayerId: string;
  punishedPlayerId: string;
  lostClothing: string | null;
  reward: string | null;
  rewardDetails: RewardDetails | null;
  isGameOver: boolean;
  winnerPlayerId: string | null;
}

export interface GameState {
  gameId: string;
  status: "WaitingForOpponent" | "InProgress" | "Finished" | "Abandoned";
  mode: "Local" | "Remote";
  joinCode: string | null;
  currentPlayerIndex: number;
  roundNumber: number;
  players: Player[];
  currentQuestion: Question | null;
  pendingRoundResult: PendingRoundResult | null;
  winnerPlayerId: string | null;
  winnerName: string | null;
  createdAt: string;
  finishedAt: string | null;
}

export interface CreateRemoteGameResult {
  gameId: string;
  joinCode: string;
  playerId: string;
  state: GameState;
}

export interface JoinGameResult {
  gameId: string;
  playerId: string;
  rejoined: boolean;
  state: GameState;
}

export interface AbandonGameResult {
  abandonedByName: string | null;
  state: GameState;
}

export interface AnswerResult {
  isCorrect: boolean;
  correctAnswerIndex: number;
  currentPlayer: Player;
  punishedPlayer: Player;
  lostClothing: string | null;
  reward: string | null;
  rewardDetails: RewardDetails | null;
  isGameOver: boolean;
  winner: Player | null;
  message: string;
  state: GameState;
}

export interface CreateGameResult {
  gameId: string;
  state: GameState;
}
