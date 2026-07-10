import type { AnswerResult } from "../types/game";

const CLOTHING_LABELS: Record<string, string> = {
  Socks: "Meias",
  Shirt: "Camiseta",
  Pants: "Calça",
  Underwear: "Cueca/Calcinha"
};

export interface GameResultProps {
  result: AnswerResult | null;
}

export function GameResult({ result }: GameResultProps) {
  if (!result) {
    return (
      <>
        <div className="result">Selecione uma resposta</div>
        <div className="prize"></div>
      </>
    );
  }

  const lostClothingLabel = result.lostClothing ? CLOTHING_LABELS[result.lostClothing] ?? result.lostClothing : null;

  return (
    <>
      <div className="result">
        {result.message}
        {lostClothingLabel ? ` | ${result.punishedPlayer.name} perdeu: ${lostClothingLabel}` : ""}
      </div>
      <div className="prize">{result.reward ?? ""}</div>
    </>
  );
}
