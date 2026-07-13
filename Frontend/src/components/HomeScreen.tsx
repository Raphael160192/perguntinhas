export interface HomeScreenProps {
  onLocal: () => void;
  onRemoteCreate: () => void;
  onRemoteJoin: () => void;
}

export function HomeScreen({ onLocal, onRemoteCreate, onRemoteJoin }: HomeScreenProps) {
  return (
    <div className="screen screen--question">
      <div className="setup">
        <div className="setup__brand">perguntinhas</div>
        <div className="setup__title">Como vocês vão jogar?</div>
        <div className="setup__subtitle">
          No mesmo aparelho, passando o celular de mão em mão — ou cada um no seu, com as telas
          sincronizadas.
        </div>

        <div className="setup__fields">
          <button className="btn btn--premium" onClick={onLocal}>
            Jogar neste aparelho
          </button>
          <button className="btn btn--primary" onClick={onRemoteCreate}>
            Criar sala para dois aparelhos
          </button>
          <button className="btn btn--ghost" onClick={onRemoteJoin}>
            Entrar numa sala
          </button>
        </div>
      </div>
    </div>
  );
}
