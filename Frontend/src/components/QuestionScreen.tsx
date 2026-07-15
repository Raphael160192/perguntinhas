import type { GameState } from "../types/game";
import { TurnHeader } from "./TurnHeader";
import { OptionCard } from "./OptionCard";
import { GameMenu } from "./GameMenu";

export interface QuestionScreenProps {
  state: GameState;
  selectedOptionIndex: number | null;
  loading: boolean;
  // false quando este aparelho é o espectador (modo remoto, vez do outro jogador).
  interactive: boolean;
  onSelect: (index: number) => void;
  onConfirm: () => void;
  onRestart: () => void;
  onAbandon: () => void;
}

export function QuestionScreen({
  state,
  selectedOptionIndex,
  loading,
  interactive,
  onSelect,
  onConfirm,
  onRestart,
  onAbandon
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
        <div className="theme-row__right">
          <div className="round-counter">RODADA {state.roundNumber}</div>
          <GameMenu onRestart={onRestart} onAbandon={onAbandon} loading={loading} />
        </div>
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
