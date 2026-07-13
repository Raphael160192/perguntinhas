import { useState } from "react";

export interface RemoteJoinProps {
  onJoin: (joinCode: string, playerName: string) => void;
  onBack: () => void;
  loading: boolean;
  error: string | null;
}

export function RemoteJoin({ onJoin, onBack, loading, error }: RemoteJoinProps) {
  const [name, setName] = useState("");
  const [code, setCode] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!code.trim()) return;
    onJoin(code.trim().toUpperCase(), name.trim() || "Jogador 2");
  }

  return (
    <div className="screen screen--question">
      <form className="setup" onSubmit={handleSubmit}>
        <div className="setup__brand">perguntinhas</div>
        <div className="setup__title">Entrar numa sala</div>
        <div className="setup__subtitle">Digite o código que apareceu no aparelho de quem criou.</div>

        <div className="setup__fields">
          <div className="setup__field">
            <label htmlFor="join-code">Código da sala</label>
            <input
              id="join-code"
              value={code}
              onChange={(e) => setCode(e.target.value.toUpperCase())}
              placeholder="EX: AB37"
              maxLength={4}
              autoComplete="off"
              style={{ textTransform: "uppercase", letterSpacing: "6px", fontWeight: 800 }}
            />
          </div>
          <div className="setup__field">
            <label htmlFor="joiner-name">Seu nome</label>
            <input
              id="joiner-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Nome"
              maxLength={20}
              autoComplete="off"
            />
          </div>
        </div>

        {error && <div className="error-note">{error}</div>}

        <button type="submit" className="btn btn--premium" disabled={loading || !code.trim()}>
          {loading ? "Entrando…" : "Entrar na sala"}
        </button>
        <button type="button" className="btn btn--ghost" onClick={onBack} disabled={loading}>
          Voltar
        </button>
      </form>
    </div>
  );
}
