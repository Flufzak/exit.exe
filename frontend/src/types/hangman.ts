export type SessionStatus = "InProgress" | "Success" | "Failed" | string;

export type SessionNarrativeDto = {
  intro: string;
  success: string;
  failure: string;
};

export type SessionDto = {
  sessionId: string;
  gameType: string;
  maskedWord: string;
  attemptsLeft: number;
  guessedLetters: string[];
  status: SessionStatus;
  narrative?: SessionNarrativeDto | null;
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
  language: string;
};

export type GuessRequest = {
  letter: string;
};