import type { ClothingState } from "../types/game";

// Ordem de perda usada pelo backend — apenas para exibição dos pips.
const CLOTHING_ORDER: (keyof ClothingState)[] = ["socks", "shirt", "pants", "underwear"];

export interface ClothingPipsProps {
  clothes: ClothingState;
  color: string;
  large?: boolean;
}

export function ClothingPips({ clothes, color, large = false }: ClothingPipsProps) {
  return (
    <span className={"pips" + (large ? " pips--lg" : "")}>
      {CLOTHING_ORDER.map((key) => {
        const owned = clothes[key];
        return (
          <span
            key={key}
            className="pip"
            style={
              owned
                ? { background: color }
                : { background: "transparent", border: `1px solid ${color}73` }
            }
          />
        );
      })}
    </span>
  );
}
