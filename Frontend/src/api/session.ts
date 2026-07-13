// Identidade da sessão de jogo persistida no aparelho (localStorage).
// Permite retomar a partida após refresh ou retorno acidental à home.

export interface SavedSession {
  gameId: string;
  playerId: string | null; // null = modo local
  joinCode: string | null;
  playerName: string | null;
}

const STORAGE_KEY = "perguntinhas.session";

export function saveSession(session: SavedSession): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } catch {
    // armazenamento indisponível (modo privado etc.) — o jogo segue sem retomada
  }
}

export function loadSession(): SavedSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as SavedSession;
    return parsed.gameId ? parsed : null;
  } catch {
    return null;
  }
}

export function clearSession(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // nada a fazer
  }
}
