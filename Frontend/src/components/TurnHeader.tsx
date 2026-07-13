import type { GameState } from "../types/game";
import { Scoreboard } from "./Scoreboard";

export interface TurnHeaderProps {
  state: GameState;
}

export function TurnHeader({ state }: TurnHeaderProps) {
  const currentPlayer = state.players[state.currentPlayerIndex];
  const initial = currentPlayer.name.charAt(0).toUpperCase();

  return (
    <div className="turn-header">
      <div className="turn-header__player">
        <div className={"avatar" + (state.currentPlayerIndex === 1 ? " avatar--premium" : "")}>
          {initial}
        </div>
        <div>
          <div className="turn-header__label">VEZ DE</div>
          <div className="turn-header__name">{currentPlayer.name}</div>
        </div>
      </div>
      <Scoreboard state={state} />
    </div>
  );
}
