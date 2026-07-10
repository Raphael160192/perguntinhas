import type { Question } from "../types/game";

export interface QuestionCardProps {
  question: Question;
  selectedOptionIndex: number | null;
  disabled: boolean;
  onSelect: (index: number) => void;
}

export function QuestionCard({ question, selectedOptionIndex, disabled, onSelect }: QuestionCardProps) {
  return (
    <div>
      <div className="question">{question.text}</div>
      <div className="options">
        {question.options.map((option, index) => (
          <div className="option" key={index}>
            <input
              type="radio"
              name="option"
              id={`option${index}`}
              checked={selectedOptionIndex === index}
              disabled={disabled}
              onChange={() => onSelect(index)}
            />
            <label htmlFor={`option${index}`}>{option}</label>
          </div>
        ))}
      </div>
    </div>
  );
}
