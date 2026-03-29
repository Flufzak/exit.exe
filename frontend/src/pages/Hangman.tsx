import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import HangmanGame from "../components/hangman/HangmanGame";
import {
  getHangmanSession,
  requestHangmanHint,
  startHangmanSession,
  submitHangmanGuess,
} from "../api/hangmanApi";
import { useStories } from "../hooks/useStories";
import type { GuessResultDto, SessionDto } from "../types/hangman";

const HANGMAN_DURATION_SECONDS = 240;
const MAX_HINTS = 3;

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
    narrative: previous.narrative,
    hintsUsed: previous.hintsUsed,
    score: previous.score,
  };
}

function normalizeLanguage(language: string): "nl" | "en" {
  return language.startsWith("nl") ? "nl" : "en";
}

export default function HangmanPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const storyId = searchParams.get("story") ?? "kazimir";
  const { i18n } = useTranslation();

  const { stories, loading: storiesLoading, error: storiesError } = useStories();

  const story = useMemo(
    () => stories.find((item) => item.id.toLowerCase() === storyId.toLowerCase()) ?? null,
    [stories, storyId],
  );

  const [session, setSession] = useState<SessionDto | null>(null);
  const [sessionLoading, setSessionLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [sessionError, setSessionError] = useState<string | null>(null);
  const [hints, setHints] = useState<string[]>([]);
  const [timeLeft, setTimeLeft] = useState(HANGMAN_DURATION_SECONDS);

  const hasStartedSessionRef = useRef(false);
  const hasNavigatedRef = useRef(false);

  const startNewSession = useCallback(async () => {
    setSessionLoading(true);
    setSessionError(null);
    setHints([]);
    setTimeLeft(HANGMAN_DURATION_SECONDS);
    setSession(null);
    hasNavigatedRef.current = false;

    try {
      const language = normalizeLanguage(i18n.language);

      const result = await startHangmanSession({
        gameType: "hangman",
        language,
      });

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
  }, [i18n.language]);

  useEffect(() => {
    if (hasStartedSessionRef.current) {
      return;
    }

    hasStartedSessionRef.current = true;
    void startNewSession();
  }, [startNewSession]);

  useEffect(() => {
    if (sessionLoading || !session || hasNavigatedRef.current) {
      return;
    }

    if (session.status === "Failed") {
      hasNavigatedRef.current = true;
      navigate("/lost", {
        state: {
          sessionId: session.sessionId,
          timeLeft,
        },
        replace: true,
      });
      return;
    }

    if (session.status === "Success") {
      hasNavigatedRef.current = true;
      navigate("/won", {
        state: {
          sessionId: session.sessionId,
          timeLeft,
        },
        replace: true,
      });
    }
  }, [session, sessionLoading, navigate, timeLeft]);

  const handleGuess = async (letter: string) => {
    if (!session || isSubmitting || timeLeft <= 0) {
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
    if (!session || isSubmitting || timeLeft <= 0) {
      return;
    }

    if ((session.hintsUsed ?? 0) >= MAX_HINTS) {
      return;
    }

    setIsSubmitting(true);
    setSessionError(null);

    try {
      const result = await requestHangmanHint(session.sessionId);

      setHints((previous) => [...previous, result.hint]);

      const refreshed = await getHangmanSession(session.sessionId);
      setSession(refreshed);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "";

      if (errorMessage === "UNAUTHORIZED") {
        setSessionError("Je sessie is niet geautoriseerd. Log opnieuw in.");
      } else {
        try {
          const refreshed = await getHangmanSession(session.sessionId);
          setSession(refreshed);
        } catch {
          console.warn("Kon sessie niet verversen na hintfout.");
        }
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRetry = async () => {
    hasStartedSessionRef.current = false;
    hasNavigatedRef.current = false;
    await startNewSession();
    hasStartedSessionRef.current = true;
  };

  const handleTimeExpired = useCallback(() => {
    if (!session || hasNavigatedRef.current) {
      return;
    }

    hasNavigatedRef.current = true;

    navigate("/lost", {
      state: {
        sessionId: session.sessionId,
        timeLeft: 0,
      },
      replace: true,
    });
  }, [navigate, session]);

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
        hints={hints}
        onGuess={handleGuess}
        onHint={handleHint}
        onRetry={handleRetry}
        onTimeUpdate={setTimeLeft}
        onTimeExpired={handleTimeExpired}
      />
    </main>
  );
}