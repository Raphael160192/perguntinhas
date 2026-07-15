import { useState } from "react";

export interface GameMenuProps {
  onRestart: () => void;
  onAbandon: () => void;
  loading: boolean;
}

type MenuView = "closed" | "menu" | "confirm-restart" | "confirm-abandon";

// Botão discreto no header que abre o menu da partida (reiniciar / encerrar).
export function GameMenu({ onRestart, onAbandon, loading }: GameMenuProps) {
  const [view, setView] = useState<MenuView>("closed");

  function close() {
    setView("closed");
  }

  return (
    <>
      <button
        type="button"
        className="game-menu__trigger"
        aria-label="Menu da partida"
        onClick={() => setView("menu")}
      >
        ⋯
      </button>

      {view !== "closed" && (
        <div className="game-menu__overlay" onClick={close}>
          <div className="game-menu__sheet" onClick={(e) => e.stopPropagation()}>
            <div className="prize-sheet__handle" />

            {view === "menu" && (
              <>
                <div className="prize-sheet__label">MENU DA PARTIDA</div>
                <div className="game-menu__actions">
                  <button className="btn btn--ghost" onClick={() => setView("confirm-restart")}>
                    Reiniciar partida
                  </button>
                  <button className="btn btn--ghost" onClick={() => setView("confirm-abandon")}>
                    Encerrar partida
                  </button>
                  <button className="btn btn--primary" onClick={close}>
                    Continuar jogando
                  </button>
                </div>
              </>
            )}

            {view === "confirm-restart" && (
              <>
                <div className="prize-sheet__label">REINICIAR PARTIDA</div>
                <div className="prize-sheet__note">
                  O placar volta para 12 × 12 e todas as peças são devolvidas. Vale para os dois.
                </div>
                <div className="game-menu__actions">
                  <button
                    className="btn btn--premium"
                    disabled={loading}
                    onClick={() => {
                      close();
                      onRestart();
                    }}
                  >
                    Sim, reiniciar
                  </button>
                  <button className="btn btn--ghost" onClick={close} disabled={loading}>
                    Voltar
                  </button>
                </div>
              </>
            )}

            {view === "confirm-abandon" && (
              <>
                <div className="prize-sheet__label">ENCERRAR PARTIDA</div>
                <div className="prize-sheet__note">
                  A partida termina para os dois jogadores e não pode ser retomada.
                </div>
                <div className="game-menu__actions">
                  <button
                    className="btn btn--primary"
                    disabled={loading}
                    onClick={() => {
                      close();
                      onAbandon();
                    }}
                  >
                    Sim, encerrar
                  </button>
                  <button className="btn btn--ghost" onClick={close} disabled={loading}>
                    Voltar
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </>
  );
}
