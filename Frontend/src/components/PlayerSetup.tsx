import { useState } from "react";

export interface PlayerSetupProps {
  onStart: (player1Name: string, player2Name: string) => void;
  loading: boolean;
  error: string | null;
}

export function PlayerSetup({ onStart, loading, error }: PlayerSetupProps) {
  const [player1, setPlayer1] = useState("");
  const [player2, setPlayer2] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onStart(player1.trim() || "Jogador 1", player2.trim() || "Jogador 2");
  }

  return (
    <div className="screen screen--question">
      <form className="setup" onSubmit={handleSubmit}>
        <div className="setup__brand">perguntinhas</div>
        <div className="setup__title">Quem vai jogar hoje?</div>
        <div className="setup__subtitle">
          Perguntas alternadas entre os dois — quem errar perde ponto, quem zerar as peças perde o
          jogo.
        </div>

        <div className="setup__fields">
          <div className="setup__field">
            <label htmlFor="player1">Jogador 1</label>
            <input
              id="player1"
              value={player1}
              onChange={(e) => setPlayer1(e.target.value)}
              placeholder="Nome"
              maxLength={20}
              autoComplete="off"
            />
          </div>
          <div className="setup__field">
            <label htmlFor="player2">Jogador 2</label>
            <input
              id="player2"
              value={player2}
              onChange={(e) => setPlayer2(e.target.value)}
              placeholder="Nome"
              maxLength={20}
              autoComplete="off"
            />
          </div>
        </div>

        {error && <div className="error-note">{error}</div>}

        <button type="submit" className="btn btn--premium" disabled={loading}>
          {loading ? "Criando partida…" : "Começar o jogo"}
        </button>
      </form>
    </div>
  );
}
