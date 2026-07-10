import type { Player } from "../types/game";
import { ClothingIcons } from "./ClothingIcons";

export interface PlayerPanelProps {
  player: Player;
  isActive: boolean;
  align?: "left" | "right";
}

export function PlayerPanel({ player, isActive, align = "left" }: PlayerPanelProps) {
  return (
    <div className={`player-panel${align === "right" ? " right" : ""}`}>
      <div>
        <span className={isActive ? "player-name-active" : "player-name"}>{player.name}</span>
        <span className="score">{player.score}</span>
      </div>
      <ClothingIcons clothes={player.clothes} />
    </div>
  );
}
