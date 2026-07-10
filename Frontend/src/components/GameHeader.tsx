import type { GameState } from "../types/game";
import { PlayerPanel } from "./PlayerPanel";

export interface GameHeaderProps {
  state: GameState;
}

export function GameHeader({ state }: GameHeaderProps) {
  const [player1, player2] = state.players;

  return (
    <div className="topbar">
      <PlayerPanel player={player1} isActive={state.currentPlayerIndex === 0} align="left" />
      <div className="theme">Tema: {state.currentQuestion?.theme ?? "-"}</div>
      <PlayerPanel player={player2} isActive={state.currentPlayerIndex === 1} align="right" />
    </div>
  );
}
