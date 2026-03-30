import { useEffect, useMemo, useState } from "react";
import styled from "styled-components";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import AppButton from "../components/ui/AppButton";
import { getHangmanSession } from "../api/hangmanApi";
import type { SessionDto } from "../types/hangman";

type WinPageLocationState = {
  sessionId?: string;
};

type SessionWithAttempts = SessionDto & {
  attempts?: number;
  wrongAttempts?: number;
  totalAttempts?: number;
};

export default function WinPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();

  const state = (location.state ?? {}) as WinPageLocationState;

  const [session, setSession] = useState<SessionWithAttempts | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!state.sessionId) {
      return;
    }

    const sessionId = state.sessionId;
    let isMounted = true;

    const loadSession = async () => {
      setLoading(true);

      try {
        const result = await getHangmanSession(sessionId);

        if (isMounted) {
          setSession(result as SessionWithAttempts);
        }
      } catch (error) {
        console.error("Could not load win page session:", error);
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    void loadSession();

    return () => {
      isMounted = false;
    };
  }, [state.sessionId]);

  const hintsUsed = useMemo(() => {
    if (!session) {
      return 0;
    }

    return typeof session.hintsUsed === "number" ? session.hintsUsed : 0;
  }, [session]);

  const attempts = useMemo(() => {
    if (!session) {
      return 0;
    }

    if (typeof session.attempts === "number") {
      return session.attempts;
    }

    if (typeof session.totalAttempts === "number") {
      return session.totalAttempts;
    }

    if (typeof session.wrongAttempts === "number") {
      return session.wrongAttempts;
    }

    return 0;
  }, [session]);

  const finalScore = useMemo(() => {
    if (!session) {
      return 0;
    }

    return typeof session.score === "number" ? session.score : 0;
  }, [session]);

  return (
    <Wrapper>
      <Overlay />

      <Content>
        <Title>{t("you-escaped")}</Title>

        <Lore>
          <p>{t("final-piece-slides")}</p>
          <p>{t("mural-aligns")}</p>
          <p>{t("crack-runs-through-wall")}</p>
          <p>{t("chains-fall-loose")}</p>
          <p>{t("you-dont-look-back")}</p>
          <p>{t("kazimir-promised")}</p>
          <p>{t("never-meant-for-you")}</p>
        </Lore>

        <Stats>
          <Stat>
            <span>{t("hints-used")}</span>
            <strong>{loading ? "..." : hintsUsed}</strong>
          </Stat>

          <Stat>
            <span>{t("attempts")}</span>
            <Success>{loading ? "..." : attempts}</Success>
          </Stat>

          <Stat>
            <span>{t("final-score")}</span>
            <strong>{loading ? "..." : finalScore}</strong>
          </Stat>
        </Stats>

        <Buttons>
          <AppButton onClick={() => navigate("/")}>
            {t("return-home")}
          </AppButton>

          <AppButton variant="secondary" onClick={() => navigate("/profile")}>
            {t("view-profile")}
          </AppButton>
        </Buttons>
      </Content>
    </Wrapper>
  );
}

/* ================= LAYOUT ================= */

const Wrapper = styled.div`
  position: relative;
  margin: -1.5rem -2rem;
  width: calc(100% + 4rem);
  min-height: calc(100vh - 0px);
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background-image: url("/images/landingImg.png");
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  --accent: #6ee7b7;
  --accent-rgb: 110 231 183;
`;

const Overlay = styled.div`
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at center, rgba(0, 0, 0, 0.3), transparent 70%),
    rgba(0, 0, 0, 0.6);
  z-index: 1;
`;

const Content = styled.div`
  position: relative;
  z-index: 2;
  min-height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 2.5rem;
  gap: 2.2rem;
`;

const Buttons = styled.div`
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  justify-content: center;
`;

/* ================= TEXT ================= */

const Title = styled.h1`
  font-size: clamp(2.2rem, 5vw, 3.2rem);
  color: #6ee7b7;
  text-shadow: 0 0 12px rgb(255, 255, 255);
`;

const Lore = styled.div`
  max-width: 640px;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;

  p {
    color: #d1d5db;
    line-height: 1.7;
    font-size: 1.05rem;
    margin: 0;
  }
`;

/* ================= STATS ================= */

const Stats = styled.div`
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  justify-content: center;
`;

const Stat = styled.div`
  padding: 1rem 1.4rem;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: rgba(0, 0, 0, 0.35);
  min-width: 120px;
  backdrop-filter: blur(4px);

  span {
    display: block;
    font-size: 0.7rem;
    color: #9ca3af;
    margin-bottom: 0.3rem;
    letter-spacing: 0.08em;
  }

  strong {
    font-size: 1.3rem;
    color: #e5e7eb;
  }
`;

const Success = styled.strong`
  color: #6ee7b7;
`;
