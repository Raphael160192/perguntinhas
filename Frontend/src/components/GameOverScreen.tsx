import type { GameState } from "../types/game";
import { ClothingPips } from "./ClothingPips";
import { PLAYER_COLORS } from "./Scoreboard";

export interface GameOverScreenProps {
  state: GameState;
  onRestart: () => void;
  onExit: () => void;
  loading: boolean;
}

export function GameOverScreen({ state, onRestart, onExit, loading }: GameOverScreenProps) {
  const winner = state.players.find((p) => p.id === state.winnerPlayerId);
  const winnerName = winner?.name ?? state.winnerName ?? "";
  const initial = winnerName.charAt(0).toUpperCase();

  return (
    <div className="screen screen--gameover">
      <div className="takeover">
        <div className="takeover__overline" style={{ color: "rgba(201,160,92,.75)" }}>
          FIM DE JOGO
        </div>
        <div className="gameover__avatar">{initial}</div>
        <div className="gameover__title">{winnerName} venceu</div>

        <div className="final-score">
          {state.players.map((player, index) => (
            <div key={player.id}>
              {index > 0 && <div className="final-score__divider" style={{ marginBottom: 12 }} />}
              <div
                className={
                  "final-score__row" +
                  (player.id === state.winnerPlayerId ? "" : " final-score__row--loser")
                }
              >
                <span className="final-score__player">
                  <span className="scoreboard__dot" style={{ background: PLAYER_COLORS[index] }} />
                  <span className="final-score__player-name">{player.name}</span>
                </span>
                <span className="final-score__right">
                  <ClothingPips clothes={player.clothes} color={PLAYER_COLORS[index]} />
                  <span className="final-score__value" style={{ color: PLAYER_COLORS[index] }}>
                    {player.score}
                  </span>
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="screen-footer">
        <button className="btn btn--premium" onClick={onRestart} disabled={loading}>
          Jogar de novo
        </button>
        <button className="btn btn--ghost" onClick={onExit} disabled={loading}>
          Encerrar e sair
        </button>
      </div>
    </div>
  );
}
