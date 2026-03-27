import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import HangmanGame from "../components/hangman/HangmanGame";
import {
  getHangmanSession,
  requestHangmanHint,
  startHangmanSession,
  submitHangmanGuess,
} from "../api/hangmanApi";
import { useStories } from "../hooks/useStories";
import type { GuessResultDto, SessionDto } from "../types/hangman";

function mergeGuessResultIntoSession(
  previous: SessionDto,
  result: GuessResultDto,
): SessionDto {
  return {
    ...previous,
    maskedWord: result.maskedWord,
    attemptsLeft: result.attemptsLeft,
    guessedLetters: result.guessedLetters,
    status: result.status,
  };
}

export default function HangmanPage() {
  const [searchParams] = useSearchParams();
  const storyId = searchParams.get("story") ?? "kazimir";

  const { stories, loading: storiesLoading, error: storiesError } = useStories();

  const story = useMemo(
    () => stories.find((item) => item.id.toLowerCase() === storyId.toLowerCase()) ?? null,
    [stories, storyId],
  );

  const [session, setSession] = useState<SessionDto | null>(null);
  const [sessionLoading, setSessionLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [sessionError, setSessionError] = useState<string | null>(null);
  const [hint, setHint] = useState<string | null>(null);

  const startNewSession = async () => {
    setSessionLoading(true);
    setSessionError(null);
    setHint(null);

    try {
      const result = await startHangmanSession({ gameType: "hangman" });
      setSession(result);
    } catch (err) {
      const message =
        err instanceof Error && err.message === "UNAUTHORIZED"
          ? "Je moet eerst ingelogd zijn om Hangman te spelen."
          : "Kon de Hangman-sessie niet starten.";

      setSessionError(message);
    } finally {
      setSessionLoading(false);
    }
  };

  useEffect(() => {
    void startNewSession();
  }, []);

  const handleGuess = async (letter: string) => {
    if (!session || isSubmitting) {
      return;
    }

    if (session.guessedLetters.includes(letter)) {
      return;
    }

    setIsSubmitting(true);
    setSessionError(null);

    try {
      const result = await submitHangmanGuess(session.sessionId, { letter });
      setSession((previous) =>
        previous ? mergeGuessResultIntoSession(previous, result) : previous,
      );
    } catch (err) {
      const message =
        err instanceof Error && err.message === "UNAUTHORIZED"
          ? "Je sessie is niet geautoriseerd. Log opnieuw in."
          : "Kon je gok niet versturen.";

      setSessionError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleHint = async () => {
    if (!session || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setSessionError(null);

    try {
      const result = await requestHangmanHint(session.sessionId);
      setHint(result.hint);

      const refreshed = await getHangmanSession(session.sessionId);
      setSession(refreshed);
    } catch (err) {
      const message =
        err instanceof Error && err.message === "UNAUTHORIZED"
          ? "Je sessie is niet geautoriseerd. Log opnieuw in."
          : "Kon geen hint ophalen.";

      setSessionError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRetry = async () => {
    await startNewSession();
  };

  const combinedError = sessionError ?? storiesError;
  const isLoading = storiesLoading || sessionLoading || isSubmitting;

  return (
    <main
      style={{
        minHeight: "100vh",
        padding: "24px",
        background:
          "radial-gradient(circle at top, rgba(48, 78, 133, 0.16), transparent 30%), #020611",
      }}
    >
      <HangmanGame
        story={story}
        session={session}
        isLoading={isLoading}
        error={combinedError}
        hint={hint}
        onGuess={handleGuess}
        onHint={handleHint}
        onRetry={handleRetry}
      />
    </main>
  );
}