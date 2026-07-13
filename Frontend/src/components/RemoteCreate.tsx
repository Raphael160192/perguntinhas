import { useState } from "react";

export interface RemoteCreateProps {
  // null enquanto a sala não foi criada; depois vira o código a exibir na espera.
  joinCode: string | null;
  onCreate: (playerName: string) => void;
  onBack: () => void;
  loading: boolean;
  error: string | null;
}

export function RemoteCreate({ joinCode, onCreate, onBack, loading, error }: RemoteCreateProps) {
  const [name, setName] = useState("");

  if (joinCode) {
    return (
      <div className="screen screen--question">
        <div className="setup" style={{ alignItems: "center", textAlign: "center" }}>
          <div className="setup__brand">perguntinhas</div>
          <div className="overline" style={{ color: "rgba(232,160,180,.65)" }}>
            CÓDIGO DA SALA
          </div>
          <div className="join-code">{joinCode}</div>
          <div className="setup__subtitle">
            Fale esse código para a outra pessoa entrar no aparelho dela.
          </div>
          <div className="waiting-dots">aguardando o outro jogador…</div>
        </div>
      </div>
    );
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onCreate(name.trim() || "Jogador 1");
  }

  return (
    <div className="screen screen--question">
      <form className="setup" onSubmit={handleSubmit}>
        <div className="setup__brand">perguntinhas</div>
        <div className="setup__title">Criar sala</div>
        <div className="setup__subtitle">
          Você recebe um código para a outra pessoa entrar do aparelho dela.
        </div>

        <div className="setup__fields">
          <div className="setup__field">
            <label htmlFor="creator-name">Seu nome</label>
            <input
              id="creator-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Nome"
              maxLength={20}
              autoComplete="off"
            />
          </div>
        </div>

        {error && <div className="error-note">{error}</div>}

        <button type="submit" className="btn btn--premium" disabled={loading}>
          {loading ? "Criando sala…" : "Criar sala"}
        </button>
        <button type="button" className="btn btn--ghost" onClick={onBack} disabled={loading}>
          Voltar
        </button>
      </form>
    </div>
  );
}
