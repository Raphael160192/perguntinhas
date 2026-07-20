export interface HomeScreenProps {
  onLocal: () => void;
  onRemoteCreate: () => void;
  onRemoteJoin: () => void;

  // Aviso informativo (ex: "Fulano saiu da partida").
  notice?: string | null;

  // Sessão salva no aparelho: oferece voltar à partida em andamento.
  resumeAvailable: boolean;
  resumeJoinCode: string | null;
  onResume: () => void;
  onAbandon: () => void;
  loading: boolean;
}

export function HomeScreen({
  onLocal,
  onRemoteCreate,
  onRemoteJoin,
  notice = null,
  resumeAvailable,
  resumeJoinCode,
  onResume,
  onAbandon,
  loading
}: HomeScreenProps) {
  return (
    <div className="screen screen--question">
      <div className="setup">
        <div className="setup__brand">perguntinhas</div>
        <div className="setup__title">Como vocês vão jogar?</div>
        <div className="setup__subtitle">
          No mesmo aparelho, passando o celular de mão em mão — ou cada um no seu, com as telas
          sincronizadas.
        </div>

        {notice && (
          <div className="resume-banner">
            <div className="resume-banner__text">{notice}</div>
          </div>
        )}

        {resumeAvailable && (
          <div className="resume-banner">
            <div className="resume-banner__text">
              Você tem uma partida em andamento
              {resumeJoinCode ? ` (sala ${resumeJoinCode})` : ""}.
            </div>
            <div className="resume-banner__actions">
              <button className="btn btn--premium" onClick={onResume} disabled={loading}>
                {loading ? "Voltando…" : "Voltar à partida"}
              </button>
              <button className="btn btn--ghost" onClick={onAbandon} disabled={loading}>
                Abandonar
              </button>
            </div>
          </div>
        )}

        <div className="setup__fields">
          <button className="btn btn--premium" onClick={onLocal} disabled={loading}>
            Jogar neste aparelho
          </button>
          <button className="btn btn--primary" onClick={onRemoteCreate} disabled={loading}>
            Criar sala para dois aparelhos
          </button>
          <button className="btn btn--ghost" onClick={onRemoteJoin} disabled={loading}>
            Entrar numa sala
          </button>
        </div>
      </div>
    </div>
  );
}
