import type { GameState } from "../types/game";
import { ClothingPips } from "./ClothingPips";

// Cores dos jogadores conforme o design: jogador 1 = accent, jogador 2 = premium.
export const PLAYER_COLORS = ["#E8A0B4", "#C9A05C"];

export interface ScoreboardProps {
  state: GameState;
}

export function Scoreboard({ state }: ScoreboardProps) {
  return (
    <div className="scoreboard">
      {state.players.map((player, index) => (
        <div
          key={player.id}
          className={
            "scoreboard__row" +
            (state.currentPlayerIndex === index ? "" : " scoreboard__row--inactive")
          }
        >
          <span className="scoreboard__dot" style={{ background: PLAYER_COLORS[index] }} />
          <span className="scoreboard__name">{player.name}</span>
          <span className="scoreboard__score" style={{ color: PLAYER_COLORS[index] }}>
            {player.score}
          </span>
          <ClothingPips clothes={player.clothes} color={PLAYER_COLORS[index]} />
        </div>
      ))}
    </div>
  );
}
