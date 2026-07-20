// Autenticação do jogador (US6 — login com Google). O Google Identity Services
// devolve um ID token; o backend valida uma única vez e emite o JWT próprio,
// guardado no aparelho (localStorage) e enviado como Bearer pelo gameApi.
// Login é opcional: jogar anônimo continua funcionando sem nada daqui.

import { API_BASE_URL } from "./gameApi";

export interface AuthUser {
  userId: string;
  email: string;
}

interface StoredAuth {
  token: string;
  user: AuthUser;
}

const STORAGE_KEY = "perguntinhas.auth";
const GSI_SCRIPT_URL = "https://accounts.google.com/gsi/client";
const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID as string | undefined;

export function getAuthToken(): string | null {
  return readStored()?.token ?? null;
}

export function getAuthUser(): AuthUser | null {
  return readStored()?.user ?? null;
}

export function logout(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // armazenamento indisponível — nada a fazer
  }
  window.google?.accounts.id.disableAutoSelect();
}

// Abre o fluxo do Google (One Tap / seletor de conta) e troca o credential
// pelo JWT do backend. Rejeita com mensagem amigável quando não configurado,
// fechado pelo usuário ou recusado pelo backend.
export async function loginWithGoogle(): Promise<AuthUser> {
  if (!GOOGLE_CLIENT_ID) {
    throw new Error("Login com Google não configurado (VITE_GOOGLE_CLIENT_ID ausente).");
  }

  await loadGsiScript();

  const credential = await new Promise<string>((resolve, reject) => {
    const google = window.google;
    if (!google) {
      reject(new Error("Não foi possível carregar o Google. Tente novamente."));
      return;
    }

    google.accounts.id.initialize({
      client_id: GOOGLE_CLIENT_ID,
      callback: (response) => resolve(response.credential)
    });

    google.accounts.id.prompt((notification) => {
      if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
        reject(new Error("Login com Google cancelado ou indisponível neste navegador."));
      }
    });
  });

  const response = await fetch(`${API_BASE_URL}/api/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken: credential })
  });

  if (!response.ok) {
    let message = "Não foi possível entrar com o Google. Tente novamente.";
    try {
      const data = await response.json();
      if (data?.message) message = data.message;
    } catch {
      // corpo não-JSON; mantém a mensagem padrão
    }
    throw new Error(message);
  }

  const result = (await response.json()) as { token: string; userId: string; email: string };
  const user: AuthUser = { userId: result.userId, email: result.email };

  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: result.token, user } satisfies StoredAuth));
  } catch {
    // sem armazenamento o login vale só até o refresh
  }

  return user;
}

function readStored(): StoredAuth | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as StoredAuth;
    return parsed.token && parsed.user?.userId ? parsed : null;
  } catch {
    return null;
  }
}

let gsiScriptPromise: Promise<void> | null = null;

function loadGsiScript(): Promise<void> {
  if (window.google) return Promise.resolve();
  if (gsiScriptPromise) return gsiScriptPromise;

  gsiScriptPromise = new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = GSI_SCRIPT_URL;
    script.async = true;
    script.onload = () => resolve();
    script.onerror = () => {
      gsiScriptPromise = null;
      reject(new Error("Não foi possível carregar o Google. Verifique a conexão."));
    };
    document.head.appendChild(script);
  });

  return gsiScriptPromise;
}
