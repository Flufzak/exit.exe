export type ProfileStats = {
  totalGamesPlayed: number;
  gamesWon: number;
  gamesLost: number;
  gamesInProgress: number;
  totalHintsUsed: number;
  bestScore: number | null;
  averageScore: number | null;
  lastPlayedAtUtc: string | null;
};
