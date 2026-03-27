export type SessionStatus = "Running" | "Won" | "Lost" | string;

export type SessionDto = {
  sessionId: string;
  gameType: string;
  maskedWord: string;
  attemptsLeft: number;
  guessedLetters: string[];
  status: SessionStatus;
};

export type GuessResultDto = {
  correct: boolean;
  maskedWord: string;
  attemptsLeft: number;
  guessedLetters: string[];
  status: SessionStatus;
};

export type HintResultDto = {
  hint: string;
  hintsUsed: number;
};

export type StartSessionRequest = {
  gameType: string;
};

export type GuessRequest = {
  letter: string;
};