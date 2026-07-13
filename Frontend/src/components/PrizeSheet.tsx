import type { AnswerResult } from "../types/game";

export interface PrizeSheetProps {
  result: AnswerResult;
  onDone: () => void;
  onSkip: () => void;
  loading: boolean;
  // false quando este aparelho é o espectador — o avanço vem por evento do outro aparelho.
  showActions?: boolean;
}

export function PrizeSheet({ result, onDone, onSkip, loading, showActions = true }: PrizeSheetProps) {
  const winnerName = result.currentPlayer.name;
  const rewardText = result.rewardDetails?.text ?? result.reward;

  return (
    <div className="screen screen--question" style={{ position: "relative" }}>
      {/* Fundo desfocado simulando a tela da pergunta */}
      <div className="prize-backdrop">
        <div className="prize-backdrop__name">{winnerName}</div>
        <div className="prize-backdrop__question">…</div>
        <div className="prize-backdrop__bar" />
        <div className="prize-backdrop__bar" />
        <div className="prize-backdrop__bar" />
      </div>

      <div className="prize-sheet">
        <div className="prize-sheet__handle" />
        <div className="prize-sheet__label">
          {result.rewardDetails
            ? `NÍVEL ${result.rewardDetails.level} · ${result.rewardDetails.levelName}`
            : "PRÊMIO DA RODADA"}
        </div>
        <div className="prize-sheet__text">
          {rewardText}
        </div>
        <div className="prize-sheet__note">Só vale se os dois toparem — pular também é jogar.</div>
        {showActions ? (
          <div className="prize-sheet__actions">
            <button className="btn btn--premium" onClick={onDone} disabled={loading}>
              Feito
            </button>
            <button className="btn btn--ghost" onClick={onSkip} disabled={loading}>
              Pular
            </button>
          </div>
        ) : (
          <div className="footer-note">aguardando {winnerName}…</div>
        )}
      </div>
    </div>
  );
}
