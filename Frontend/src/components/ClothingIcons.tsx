import type { ClothingState } from "../types/game";

// Ordem padrão de perda de peças (mesma usada pelo backend, apenas para exibição).
const CLOTHING_ORDER: (keyof ClothingState)[] = ["socks", "shirt", "pants", "underwear"];

const LABELS: Record<keyof ClothingState, string> = {
  socks: "Meias",
  shirt: "Camiseta",
  pants: "Calça",
  underwear: "Cueca/Calcinha"
};

function svgIcon(kind: keyof ClothingState): JSX.Element {
  const stroke = "currentColor";

  switch (kind) {
    case "shirt":
      return (
        <svg viewBox="0 0 24 24" fill="none" stroke={stroke} strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
          <path d="M7 6l5-3 5 3 2 3v9a2 2 0 0 1-2 2h-10a2 2 0 0 1-2-2V9l2-3z"></path>
          <path d="M9 21V9m6 12V9"></path>
        </svg>
      );
    case "pants":
      return (
        <svg viewBox="0 0 24 24" fill="none" stroke={stroke} strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
          <path d="M7 3h10l-1 6h-8L7 3z"></path>
          <path d="M9 9v12m6-12v12"></path>
          <path d="M9 21h-2l-1-8m10 8h-2l1-8"></path>
        </svg>
      );
    case "underwear":
      return (
        <svg viewBox="0 0 24 24" fill="none" stroke={stroke} strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
          <path d="M3 7h18l-2 6c-1.5 3-4 4-7 4s-5.5-1-7-4L3 7z"></path>
          <path d="M7 7c1 2 3 3 5 3s4-1 5-3"></path>
        </svg>
      );
    case "socks":
      return (
        <svg viewBox="0 0 24 24" fill="none" stroke={stroke} strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
          <path d="M8 3v9l-3 3a4 4 0 0 0 6 3l3-3V3z"></path>
          <path d="M12 8H8m8 1v5l-2 2"></path>
        </svg>
      );
  }
}

export interface ClothingIconsProps {
  clothes: ClothingState;
}

export function ClothingIcons({ clothes }: ClothingIconsProps) {
  return (
    <span className="clothing-icons">
      {CLOTHING_ORDER.map((key) => {
        const owned = clothes[key];
        return (
          <span
            key={key}
            className={"piece" + (owned ? "" : " lost")}
            title={LABELS[key] + (owned ? " (com o jogador)" : " (perdida)")}
          >
            {svgIcon(key)}
          </span>
        );
      })}
    </span>
  );
}
