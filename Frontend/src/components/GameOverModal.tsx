export interface GameOverModalProps {
  open: boolean;
  winnerName: string | null;
  onRestart: () => void;
  onClose: () => void;
}

export function GameOverModal({ open, winnerName, onRestart, onClose }: GameOverModalProps) {
  if (!open) {
    return null;
  }

  return (
    <div className="modal-overlay">
      <div className="modal">
        <h5>Fim de Jogo</h5>
        <div>{winnerName ? `${winnerName} venceu!` : "Partida finalizada."}</div>
        <div className="modal-actions">
          <button className="btn btn-primary" onClick={onRestart}>
            Reiniciar
          </button>
          <button className="btn btn-secondary" onClick={onClose}>
            Fechar
          </button>
        </div>
      </div>
    </div>
  );
}
