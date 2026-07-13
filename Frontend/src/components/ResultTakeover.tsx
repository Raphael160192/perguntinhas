import type { AnswerResult } from "../types/game";
import { ClothingPips } from "./ClothingPips";
import { PLAYER_COLORS } from "./Scoreboard";

const LETTERS = ["A", "B", "C", "D", "E", "F"];

const CLOTHING_LABELS: Record<string, string> = {
  Socks: "as meias",
  Shirt: "a camiseta",
  Pants: "a calça",
  Underwear: "a peça íntima"
};

function clothingIcon(kind: string) {
  switch (kind) {
    case "Shirt":
      return (
        <svg viewBox="0 0 24 24">
          <path d="M7 6l5-3 5 3 2 3v9a2 2 0 0 1-2 2h-10a2 2 0 0 1-2-2V9l2-3z"></path>
          <path d="M9 21V9m6 12V9"></path>
        </svg>
      );
    case "Pants":
      return (
        <svg viewBox="0 0 24 24">
          <path d="M7 3h10l-1 6h-8L7 3z"></path>
          <path d="M9 9v12m6-12v12"></path>
          <path d="M9 21h-2l-1-8m10 8h-2l1-8"></path>
        </svg>
      );
    case "Underwear":
      return (
        <svg viewBox="0 0 24 24">
          <path d="M3 7h18l-2 6c-1.5 3-4 4-7 4s-5.5-1-7-4L3 7z"></path>
          <path d="M7 7c1 2 3 3 5 3s4-1 5-3"></path>
        </svg>
      );
    default:
      return (
        <svg viewBox="0 0 24 24">
          <path d="M8 3v9l-3 3a4 4 0 0 0 6 3l3-3V3z"></path>
          <path d="M12 8H8m8 1v5l-2 2"></path>
        </svg>
      );
  }
}

export interface ResultTakeoverProps {
  result: AnswerResult;
  onContinue: () => void;
  loading: boolean;
  // false quando este aparelho é o espectador — o avanço vem por evento do outro aparelho.
  showActions?: boolean;
}

export function ResultTakeover({ result, onContinue, loading, showActions = true }: ResultTakeoverProps) {
  const punished = result.punishedPlayer;
  const punishedIndex = result.state.players.findIndex((p) => p.id === punished.id);
  const opponentName = result.state.players.find((p) => p.id !== result.currentPlayer.id)?.name;
  const rewardText = result.rewardDetails?.text ?? result.reward;

  const lossCard = result.lostClothing ? (
    <div className="loss-card">
      <div className="loss-card__icon">{clothingIcon(result.lostClothing)}</div>
      <div className="loss-card__title">
        {punished.name} perdeu {CLOTHING_LABELS[result.lostClothing] ?? result.lostClothing}
      </div>
      <ClothingPips
        clothes={punished.clothes}
        color={PLAYER_COLORS[punishedIndex] ?? PLAYER_COLORS[0]}
        large
      />
      <div className="loss-card__remaining">
        {punished.remainingClothesCount}{" "}
        {punished.remainingClothesCount === 1 ? "peça restante" : "peças restantes"}
      </div>
    </div>
  ) : null;

  if (result.isCorrect) {
    return (
      <div className="screen screen--success">
        <div className="takeover">
          <div className="takeover__overline" style={{ color: "rgba(201,160,92,.8)" }}>
            MERECIDO.
          </div>
          <div className="takeover__title takeover__title--success">Acertou!</div>
          <div className="takeover__subtitle">
            {punished.name} perdeu 1 ponto — agora está com {punished.score}.
          </div>
          {lossCard}
          {!showActions && rewardText && (
            <div className="loss-card" style={{ borderColor: "rgba(201,160,92,.4)" }}>
              <div className="overline" style={{ color: "var(--premium)" }}>
                PRÊMIO DA RODADA
              </div>
              <div className="loss-card__title">
                {rewardText}
              </div>
            </div>
          )}
        </div>
        <div className="screen-footer">
          {showActions ? (
            <button className="btn btn--premium" onClick={onContinue} disabled={loading}>
              {result.isGameOver ? "Abrir prêmio final" : "Abrir prêmio da rodada"}
            </button>
          ) : (
            <div className="footer-note">aguardando {result.currentPlayer.name}…</div>
          )}
          {showActions && !result.isGameOver && opponentName && (
            <div className="footer-note">depois é a vez de {opponentName}</div>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="screen screen--error">
      <div className="takeover">
        <div className="takeover__overline" style={{ color: "rgba(232,160,180,.75)" }}>
          AI, AI, AI…
        </div>
        <div className="takeover__title">Errou!</div>
        <div className="takeover__subtitle">
          A certa era a{" "}
          <strong style={{ color: "var(--accent)" }}>
            {LETTERS[result.correctAnswerIndex] ?? result.correctAnswerIndex + 1}
          </strong>
          .
        </div>
        {lossCard}
      </div>
      <div className="screen-footer">
        {showActions ? (
          <button className="btn btn--primary" onClick={onContinue} disabled={loading}>
            {result.isGameOver ? "Ver resultado" : `Vez de ${opponentName} →`}
          </button>
        ) : (
          <div className="footer-note">aguardando {result.currentPlayer.name}…</div>
        )}
      </div>
    </div>
  );
}
