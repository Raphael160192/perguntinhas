// Tipos mínimos do Google Identity Services (script https://accounts.google.com/gsi/client).
// Só o que o fluxo de login usa — o SDK completo não tem pacote de tipos oficial.

interface GoogleCredentialResponse {
  credential: string;
}

interface GoogleIdConfiguration {
  client_id: string;
  callback: (response: GoogleCredentialResponse) => void;
  cancel_on_tap_outside?: boolean;
}

interface GooglePromptMomentNotification {
  isNotDisplayed: () => boolean;
  isSkippedMoment: () => boolean;
}

interface GoogleAccountsId {
  initialize: (config: GoogleIdConfiguration) => void;
  prompt: (listener?: (notification: GooglePromptMomentNotification) => void) => void;
  renderButton: (parent: HTMLElement, options: { theme?: string; size?: string; text?: string; width?: number }) => void;
  disableAutoSelect: () => void;
}

interface Window {
  google?: {
    accounts: {
      id: GoogleAccountsId;
    };
  };
}
