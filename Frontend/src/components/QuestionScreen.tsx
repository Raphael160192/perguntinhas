import type { GameState } from "../types/game";
import { TurnHeader } from "./TurnHeader";
import { OptionCard } from "./OptionCard";

export interface QuestionScreenProps {
  state: GameState;
  round: number;
  selectedOptionIndex: number | null;
  loading: boolean;
  // false quando este aparelho é o espectador (modo remoto, vez do outro jogador).
  interactive: boolean;
  onSelect: (index: number) => void;
  onConfirm: () => void;
}

export function QuestionScreen({
  state,
  round,
  selectedOptionIndex,
  loading,
  interactive,
  onSelect,
  onConfirm
}: QuestionScreenProps) {
  const question = state.currentQuestion;
  if (!question) {
    return null;
  }

  const currentPlayerName = state.players[state.currentPlayerIndex]?.name;

  // Tap seleciona; tap de novo na mesma alternativa confirma e envia para a API.
  function handleOptionClick(index: number) {
    if (selectedOptionIndex === index) {
      onConfirm();
    } else {
      onSelect(index);
    }
  }

  return (
    <div className="screen screen--question">
      <TurnHeader state={state} />

      <div className="theme-row">
        <div className="theme-pill">
          {question.theme} · Nível {question.level}
        </div>
        <div className="round-counter">RODADA {round}</div>
      </div>

      <div className="question-text">{question.text}</div>

      <div className="options">
        {question.options.map((option, index) => (
          <OptionCard
            key={index}
            index={index}
            text={option}
            selected={selectedOptionIndex === index}
            disabled={loading || !interactive}
            onClick={() => handleOptionClick(index)}
          />
        ))}
      </div>

      <div className="question-hint">
        {!interactive
          ? `vez de ${currentPlayerName}…`
          : selectedOptionIndex === null
            ? "um toque seleciona · sem voltar atrás"
            : "toque de novo para confirmar"}
      </div>
    </div>
  );
}
