const LETTERS = ["A", "B", "C", "D", "E", "F"];

export interface OptionCardProps {
  index: number;
  text: string;
  selected: boolean;
  disabled: boolean;
  onClick: () => void;
}

export function OptionCard({ index, text, selected, disabled, onClick }: OptionCardProps) {
  return (
    <button
      type="button"
      className={"option-card" + (selected ? " option-card--selected" : "")}
      disabled={disabled}
      onClick={onClick}
    >
      <span className="option-card__letter">{LETTERS[index] ?? index + 1}</span>
      <span className="option-card__text">{text}</span>
    </button>
  );
}
