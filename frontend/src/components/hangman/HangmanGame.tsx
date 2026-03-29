import { useEffect, useRef, useState } from "react";
import "../../styles/HangmanGame.css";
import HangmanFigure from "./HangmanFigure";
import type { SessionDto } from "../../types/hangman";

const ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("");
const DEFAULT_DURATION_MINUTES = 4;
const FINAL_WARNING_SECONDS = 30;
const MAX_HINTS = 3;

type HangmanStoryViewModel = {
  description?: string | null;
  difficulty?: string | null;
  durationMinutes?: number | null;
  slug?: string | null;
  title?: string | null;
};

type HangmanGameProps = {
  error?: string | null;
  hints?: string[];
  isLoading?: boolean;
  onGuess: (letter: string) => void;
  onHint: () => void;
  onRetry: () => void;
  onTimeUpdate?: (timeLeft: number) => void;
  onTimeExpired?: () => void;
  session: SessionDto | null;
  story: HangmanStoryViewModel | null;
};

function formatSeconds(totalSeconds: number): string {
  const safeValue = Math.max(0, totalSeconds);
  const minutes = Math.floor(safeValue / 60);
  const seconds = safeValue % 60;

  return `${minutes.toString().padStart(2, "0")}:${seconds
    .toString()
    .padStart(2, "0")}`;
}

function getAttemptsLeft(session: SessionDto | null): number {
  if (!session) {
    return 6;
  }

  return typeof session.attemptsLeft === "number" ? session.attemptsLeft : 6;
}

function getFailedAttempts(session: SessionDto | null): number {
  if (!session) {
    return 0;
  }

  return Math.max(0, 6 - getAttemptsLeft(session));
}

function getMaskedWord(session: SessionDto | null): string {
  if (!session) {
    return "_ _ _ _ _";
  }

  return typeof session.maskedWord === "string" && session.maskedWord.length > 0
    ? session.maskedWord
    : "_ _ _ _ _";
}

function getGuessedLetters(session: SessionDto | null): string[] {
  if (!session || !Array.isArray(session.guessedLetters)) {
    return [];
  }

  return session.guessedLetters.map((letter) => letter.toUpperCase());
}

function getStatus(session: SessionDto | null): string {
  if (!session) {
    return "InProgress";
  }

  return typeof session.status === "string" && session.status.length > 0
    ? session.status
    : "InProgress";
}

function getHintsUsed(session: SessionDto | null): number {
  if (!session) {
    return 0;
  }

  return typeof session.hintsUsed === "number" ? session.hintsUsed : 0;
}

function getTitle(story: HangmanStoryViewModel | null): string {
  if (!story) {
    return "UNKNOWN STORY";
  }

  if (story.title && story.title.trim().length > 0) {
    return story.title;
  }

  if (story.slug && story.slug.trim().length > 0) {
    return story.slug.toUpperCase();
  }

  return "UNKNOWN STORY";
}

function getDescription(story: HangmanStoryViewModel | null): string {
  if (story?.description && story.description.trim().length > 0) {
    return story.description;
  }

  return "A forgotten chamber holds the next key. Read the signs. Break the code. Escape before time runs out.";
}

function getDifficulty(story: HangmanStoryViewModel | null): string {
  if (story?.difficulty && story.difficulty.trim().length > 0) {
    return story.difficulty;
  }

  return "Hard";
}

function getDurationMinutes(story: HangmanStoryViewModel | null): number {
  if (typeof story?.durationMinutes !== "number" || story.durationMinutes <= 0) {
    return DEFAULT_DURATION_MINUTES;
  }

  return story.durationMinutes;
}

function getGameMasterMessage(
  status: string,
  failedAttempts: number,
  hints: string[],
  isLoading: boolean,
  hasTimedOut: boolean,
  timeLeft: number,
  hasStartedTimer: boolean,
): string {
  if (isLoading) {
    return "The dungeon listens... preparing the next phase.";
  }

  if (!hasStartedTimer) {
    return "The chamber waits. Your first chosen letter will begin the ritual clock.";
  }

  if (hasTimedOut) {
    return "Too late. The ritual is complete. The chamber has chosen silence over mercy.";
  }

  if (status === "Success") {
    return "You broke the code. The prison seal weakens.";
  }

  if (status === "Failed") {
    return "The chamber has judged you. The mechanism is complete.";
  }

  if (timeLeft <= FINAL_WARNING_SECONDS) {
    return "The final seconds strike like hammers. Move now or be buried here.";
  }

  if (hints.length > 0) {
    return `A whisper from the Game Master: ${hints[hints.length - 1]}`;
  }

  if (failedAttempts >= 5) {
    return "One more mistake and the cell claims you.";
  }

  if (failedAttempts >= 3) {
    return "The chains tighten. Think before you choose again.";
  }

  return "Choose carefully. Every wrong letter feeds the mechanism.";
}

function getStatusLabel(
  status: string,
  hasTimedOut: boolean,
  hasStartedTimer: boolean,
): string {
  if (!hasStartedTimer) {
    return "READY";
  }

  if (hasTimedOut) {
    return "TIME EXPIRED";
  }

  if (status === "Success") {
    return "CODE BROKEN";
  }

  if (status === "Failed") {
    return "EXECUTION COMPLETE";
  }

  return "MECHANISM ACTIVE";
}

function getNarrativeMessage(
  session: SessionDto | null,
  status: string,
): string | null {
  if (!session?.narrative) {
    return null;
  }

  if (status === "Success" && session.narrative.success.trim().length > 0) {
    return session.narrative.success;
  }

  if (status === "Failed" && session.narrative.failure.trim().length > 0) {
    return session.narrative.failure;
  }

  if (status === "InProgress" && session.narrative.intro.trim().length > 0) {
    return session.narrative.intro;
  }

  return null;
}

function getSessionKey(
  session: SessionDto | null,
  story: HangmanStoryViewModel | null,
): string {
  const storyKey = story?.slug?.trim() || story?.title?.trim() || "unknown-story";
  const sessionId =
    typeof session?.sessionId === "string" && session.sessionId.length > 0
      ? session.sessionId
      : "no-session";

  return `${storyKey}-${sessionId}`;
}

export default function HangmanGame({
  error,
  hints = [],
  isLoading = false,
  onGuess,
  onHint,
  onRetry,
  onTimeUpdate,
  onTimeExpired,
  session,
  story,
}: HangmanGameProps) {
  const attemptsLeft = getAttemptsLeft(session);
  const failedAttempts = getFailedAttempts(session);
  const guessedLetters = getGuessedLetters(session);
  const maskedWord = getMaskedWord(session);
  const status = getStatus(session);
  const hintsUsed = getHintsUsed(session);
  const hintsRemaining = Math.max(0, MAX_HINTS - hintsUsed);
  const outOfHints = hintsUsed >= MAX_HINTS;
  const difficulty = getDifficulty(story);
  const durationMinutes = getDurationMinutes(story);
  const totalTimeInSeconds = durationMinutes * 60;
  const sessionKey = getSessionKey(session, story);

  const [timeLeft, setTimeLeft] = useState(totalTimeInSeconds);
  const [hasTimedOut, setHasTimedOut] = useState(false);
  const [hasStartedTimer, setHasStartedTimer] = useState(false);

  const clock1AudioRef = useRef<HTMLAudioElement | null>(null);
  const clock2AudioRef = useRef<HTMLAudioElement | null>(null);
  const timeoutAudioRef = useRef<HTMLAudioElement | null>(null);
  const previousSessionKeyRef = useRef<string | null>(null);
  const activeClockRef = useRef<"clock1" | "clock2" | "none">("none");
  const timeoutSoundPlayedRef = useRef(false);
  const audioUnlockedRef = useRef(false);

  const isFinished = status === "Success" || status === "Failed";
  const isGameOver = isFinished || hasTimedOut;

  function unlockAudio() {
    if (audioUnlockedRef.current) {
      return;
    }

    audioUnlockedRef.current = true;

    const audioElements = [
      clock1AudioRef.current,
      clock2AudioRef.current,
      timeoutAudioRef.current,
    ];

    for (const audio of audioElements) {
      if (!audio) {
        continue;
      }

      audio.volume = 0;

      audio
        .play()
        .then(() => {
          audio.pause();
          audio.currentTime = 0;
          audio.volume = 1;
        })
        .catch(() => {
          audio.volume = 1;
        });
    }
  }

  useEffect(() => {
    clock1AudioRef.current = new Audio("/assets/sfx/clock1.mp3");
    clock2AudioRef.current = new Audio("/assets/sfx/clock2.mp3");
    timeoutAudioRef.current = new Audio("/assets/sfx/time-up.mp3");

    if (clock1AudioRef.current) {
      clock1AudioRef.current.loop = true;
      clock1AudioRef.current.preload = "auto";
    }

    if (clock2AudioRef.current) {
      clock2AudioRef.current.loop = true;
      clock2AudioRef.current.preload = "auto";
    }

    if (timeoutAudioRef.current) {
      timeoutAudioRef.current.preload = "auto";
    }

    return () => {
      if (clock1AudioRef.current) {
        clock1AudioRef.current.pause();
        clock1AudioRef.current.currentTime = 0;
      }

      if (clock2AudioRef.current) {
        clock2AudioRef.current.pause();
        clock2AudioRef.current.currentTime = 0;
      }

      if (timeoutAudioRef.current) {
        timeoutAudioRef.current.pause();
        timeoutAudioRef.current.currentTime = 0;
      }
    };
  }, []);

  useEffect(() => {
    const isFirstRender = previousSessionKeyRef.current === null;
    const isNewSession = previousSessionKeyRef.current !== sessionKey;

    if (isFirstRender || isNewSession) {
      setTimeLeft(totalTimeInSeconds);
      setHasTimedOut(false);
      setHasStartedTimer(false);
      timeoutSoundPlayedRef.current = false;
      activeClockRef.current = "none";
      audioUnlockedRef.current = false;

      if (clock1AudioRef.current) {
        clock1AudioRef.current.pause();
        clock1AudioRef.current.currentTime = 0;
      }

      if (clock2AudioRef.current) {
        clock2AudioRef.current.pause();
        clock2AudioRef.current.currentTime = 0;
      }

      if (timeoutAudioRef.current) {
        timeoutAudioRef.current.pause();
        timeoutAudioRef.current.currentTime = 0;
      }

      previousSessionKeyRef.current = sessionKey;
    }
  }, [sessionKey, totalTimeInSeconds]);

  useEffect(() => {
    onTimeUpdate?.(timeLeft);
  }, [timeLeft, onTimeUpdate]);

  useEffect(() => {
    if (!hasStartedTimer || isFinished || hasTimedOut || timeLeft <= 0) {
      if (clock1AudioRef.current) {
        clock1AudioRef.current.pause();
        clock1AudioRef.current.currentTime = 0;
      }

      if (clock2AudioRef.current) {
        clock2AudioRef.current.pause();
        clock2AudioRef.current.currentTime = 0;
      }

      activeClockRef.current = "none";
      return;
    }

    const shouldUseClock2 = timeLeft <= FINAL_WARNING_SECONDS;
    const nextClock = shouldUseClock2 ? "clock2" : "clock1";

    if (activeClockRef.current !== nextClock) {
      if (clock1AudioRef.current) {
        clock1AudioRef.current.pause();
        clock1AudioRef.current.currentTime = 0;
      }

      if (clock2AudioRef.current) {
        clock2AudioRef.current.pause();
        clock2AudioRef.current.currentTime = 0;
      }

      const audioToPlay =
        nextClock === "clock2" ? clock2AudioRef.current : clock1AudioRef.current;

      if (!audioToPlay) {
        activeClockRef.current = "none";
        return;
      }

      audioToPlay
        .play()
        .then(() => {
          activeClockRef.current = nextClock;
        })
        .catch(() => {
          console.warn("Clock audio playback was blocked by the browser.");
          activeClockRef.current = "none";
        });
    }
  }, [hasStartedTimer, hasTimedOut, isFinished, timeLeft]);

  useEffect(() => {
    if (!hasStartedTimer || isFinished || hasTimedOut) {
      return;
    }

    const timer = window.setInterval(() => {
      setTimeLeft((previousTime) => {
        if (previousTime <= 1) {
          window.clearInterval(timer);
          setHasTimedOut(true);
          return 0;
        }

        return previousTime - 1;
      });
    }, 1000);

    return () => window.clearInterval(timer);
  }, [hasStartedTimer, hasTimedOut, isFinished]);

  useEffect(() => {
    if (!hasStartedTimer || !hasTimedOut || timeoutSoundPlayedRef.current) {
      return;
    }

    timeoutSoundPlayedRef.current = true;

    if (clock1AudioRef.current) {
      clock1AudioRef.current.pause();
      clock1AudioRef.current.currentTime = 0;
    }

    if (clock2AudioRef.current) {
      clock2AudioRef.current.pause();
      clock2AudioRef.current.currentTime = 0;
    }

    activeClockRef.current = "none";

    timeoutAudioRef.current
      ?.play()
      .catch(() => {
        console.warn("Timeout audio playback was blocked by the browser.");
      });
  }, [hasStartedTimer, hasTimedOut]);

  useEffect(() => {
    if (!hasTimedOut) {
      return;
    }

    onTimeExpired?.();
  }, [hasTimedOut, onTimeExpired]);

  const timerValue = formatSeconds(timeLeft);
  const narrativeMessage = getNarrativeMessage(session, status);
  const gameMasterMessage =
    narrativeMessage ??
    getGameMasterMessage(
      status,
      failedAttempts,
      hints,
      isLoading,
      hasTimedOut,
      timeLeft,
      hasStartedTimer,
    );
  const statusLabel = getStatusLabel(status, hasTimedOut, hasStartedTimer);

  return (
    <section className="hangman-screen">
      <div className="hangman-overlay" />

      <div className="hangman-shell">
        <div className="hangman-topbar">
          <div className="hangman-topbar-item">
            <span className="hangman-topbar-label">Timer</span>
            <span className="hangman-topbar-value">{timerValue}</span>
          </div>

          <div className="hangman-topbar-item">
            <span className="hangman-topbar-label">Attempts left</span>
            <span className="hangman-topbar-value">{attemptsLeft}</span>
          </div>

          <div className="hangman-topbar-item">
            <span className="hangman-topbar-label">Hints</span>
            <span className="hangman-topbar-value">
              {outOfHints ? "Out of hints" : `${hintsUsed}/${MAX_HINTS}`}
            </span>
          </div>
        </div>

        <div className="hangman-grid">
          <aside className="hangman-panel hangman-panel-left">
            <div className="hangman-panel-header">Prisoner state</div>

            <div className="hangman-figure-wrap">
              <HangmanFigure failedAttempts={failedAttempts} />
            </div>

            <div className="hangman-side-stats">
              <div className="hangman-side-stat">
                <span className="hangman-side-stat-label">Failed attempts</span>
                <span className="hangman-side-stat-value">{failedAttempts}/6</span>
              </div>

              <div className="hangman-side-stat">
                <span className="hangman-side-stat-label">Hints remaining</span>
                <span className="hangman-side-stat-value">
                  {outOfHints ? "Out of hints" : `${hintsRemaining}/${MAX_HINTS}`}
                </span>
              </div>

              <div className="hangman-side-stat">
                <span className="hangman-side-stat-label">Status</span>
                <span className="hangman-side-stat-value">
                  {hasTimedOut ? "TimedOut" : status}
                </span>
              </div>
            </div>
          </aside>

          <main className="hangman-panel hangman-panel-center">
            <div className="hangman-story-card">
              <div className="hangman-story-meta">
                <div>
                  <div className="hangman-story-label">Story</div>
                  <h1 className="hangman-story-title">{getTitle(story)}</h1>
                  <div className="hangman-story-subtitle">
                    {difficulty} · {durationMinutes} min · Live
                  </div>
                </div>

                <div className="hangman-alert-chip">{statusLabel}</div>
              </div>

              <p className="hangman-story-description">{getDescription(story)}</p>
            </div>

            <div className="hangman-game-card">
              {error ? (
                <div className="hangman-error-block">
                  <p className="hangman-error-text">{error}</p>
                  <button
                    className="hangman-secondary-button"
                    onClick={onRetry}
                    type="button"
                  >
                    Retry
                  </button>
                </div>
              ) : hasTimedOut ? (
                <div className="hangman-error-block">
                  <p className="hangman-error-text">
                    Time is up. The chamber seals shut before you can finish the code.
                  </p>
                  <button
                    className="hangman-secondary-button"
                    onClick={onRetry}
                    type="button"
                  >
                    Retry
                  </button>
                </div>
              ) : (
                <>
                  <div className="hangman-word-wrap">
                    <div className="hangman-word">{maskedWord}</div>
                  </div>

                  <div className="hangman-keyboard">
                    {ALPHABET.map((letter) => {
                      const isUsed = guessedLetters.includes(letter);

                      return (
                        <button
                          className={`hangman-key ${isUsed ? "hangman-key-used" : ""}`}
                          disabled={isLoading || isGameOver || isUsed}
                          key={letter}
                          onClick={() => {
                            if (!hasStartedTimer) {
                              unlockAudio();
                              setHasStartedTimer(true);
                            }

                            onGuess(letter);
                          }}
                          type="button"
                        >
                          {letter}
                        </button>
                      );
                    })}
                  </div>

                  <div className="hangman-bottom-row">
                    <div className="hangman-used-letters">
                      <span className="hangman-used-letters-label">Used letters</span>
                      <span className="hangman-used-letters-value">
                        {guessedLetters.length > 0
                          ? guessedLetters.join(", ")
                          : "None yet"}
                      </span>
                    </div>

                    <div className="hangman-used-letters">
                      <span className="hangman-used-letters-label">Hints</span>
                      <span className="hangman-used-letters-value">
                        {outOfHints ? "Out of hints" : `${hintsUsed}/${MAX_HINTS}`}
                      </span>
                    </div>

                    <button
                      className="hangman-primary-button"
                      disabled={isLoading || isGameOver || outOfHints}
                      onClick={onHint}
                      type="button"
                    >
                      {outOfHints ? "Out of hints" : "Request hint"}
                    </button>
                  </div>
                </>
              )}
            </div>
          </main>

          <aside className="hangman-panel hangman-panel-right">
            <div className="hangman-panel-header">Ritual Voice</div>

            <div className="hangman-master-card">
              <div className="hangman-master-seal" aria-hidden="true">
                ✠
              </div>

              <div className="hangman-master-content">
                <div className="hangman-master-title">The Whisperer</div>
                <p className="hangman-master-message">{gameMasterMessage}</p>
              </div>
            </div>

            {hints.length > 0 && !error && !hasTimedOut && (
              <div className="hangman-hint-card">
                <div className="hangman-hint-label">Hint history</div>

                <div className="hangman-hint-history-scroll">
                  {[...hints]
                    .map((hintText, index) => ({
                      text: hintText,
                      hintNumber: index + 1,
                    }))
                    .reverse()
                    .map((item, index, reversedHints) => (
                      <div
                        className="hangman-hint-history-item"
                        key={`${item.hintNumber}-${item.text}`}
                      >
                        <strong className="hangman-hint-history-title">
                          Hint {item.hintNumber}
                        </strong>

                        <div className="hangman-hint-history-body">{item.text}</div>

                        {index < reversedHints.length - 1 && (
                          <div className="hangman-hint-history-divider" />
                        )}
                      </div>
                    ))}
                </div>
              </div>
            )}
          </aside>
        </div>
      </div>
    </section>
  );
}