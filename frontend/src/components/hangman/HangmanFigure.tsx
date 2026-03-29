type HangmanFigureProps = {
  failedAttempts: number;
  maxAttempts?: number;
};

export default function HangmanFigure({
  failedAttempts,
  maxAttempts = 6,
}: HangmanFigureProps) {
  const stroke = 4;

  const accent = "var(--accent)";
  const danger = "var(--danger)";
  const gallows = "var(--border)";

  const color = failedAttempts >= maxAttempts - 1 ? danger : accent;

  const parts = [
    <circle
      key="head"
      className="hangman-draw-circle"
      cx="140"
      cy="70"
      r="20"
      stroke={color}
      strokeWidth={stroke}
      fill="none"
    />,
    <line
      key="body"
      className="hangman-draw-line"
      x1="140"
      y1="90"
      x2="140"
      y2="150"
      stroke={color}
      strokeWidth={stroke}
    />,
    <line
      key="leftArm"
      className="hangman-draw-line"
      x1="140"
      y1="110"
      x2="110"
      y2="130"
      stroke={color}
      strokeWidth={stroke}
    />,
    <line
      key="rightArm"
      className="hangman-draw-line"
      x1="140"
      y1="110"
      x2="170"
      y2="130"
      stroke={color}
      strokeWidth={stroke}
    />,
    <line
      key="leftLeg"
      className="hangman-draw-line"
      x1="140"
      y1="150"
      x2="115"
      y2="190"
      stroke={color}
      strokeWidth={stroke}
    />,
    <line
      key="rightLeg"
      className="hangman-draw-line"
      x1="140"
      y1="150"
      x2="165"
      y2="190"
      stroke={color}
      strokeWidth={stroke}
    />,
  ];

  return (
    <svg
      className="hangman-figure-svg"
      viewBox="0 0 200 250"
      preserveAspectRatio="xMidYMid meet"
      aria-label="Hangman"
      role="img"
    >
      <line x1="30" y1="230" x2="130" y2="230" stroke={gallows} strokeWidth={stroke} />
      <line x1="70" y1="230" x2="70" y2="30" stroke={gallows} strokeWidth={stroke} />
      <line x1="70" y1="30" x2="140" y2="30" stroke={gallows} strokeWidth={stroke} />
      <line x1="140" y1="30" x2="140" y2="50" stroke={gallows} strokeWidth={stroke} />

      {parts.slice(0, failedAttempts)}
    </svg>
  );
}