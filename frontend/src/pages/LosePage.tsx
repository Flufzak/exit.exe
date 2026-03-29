import styled from "styled-components";
import { useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import AppButton from "../components/ui/AppButton";

export default function LosePage() {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const location = useLocation();

  const hintsUsed = location.state?.hintsUsed ?? 0;
  const time = location.state?.time ?? "0:00";
  const score = location.state?.score ?? 0;

  return (
    <Wrapper>
      <Overlay />

      <Content>
        <Title>{t("not-escaped")}</Title>

        <Lore>
          <p>{t("bells")}</p>
          <p>{t("priests-do-not-rush")}</p>
        </Lore>

        <Stats>
          <Stat>
            <span>{t("hints-used")}</span>
            <strong>{hintsUsed}</strong>
          </Stat>

          <Stat>
            <span>{t("time")}</span>
            <Danger>{time}</Danger>
          </Stat>

          <Stat>
            <span>{t("score")}</span>
            <strong>{score}</strong>
          </Stat>
        </Stats>

        <Buttons>
          <AppButton onClick={() => navigate("/")}>
            {t("return-home")}
          </AppButton>

          <AppButton variant="secondary" onClick={() => navigate("/")}>
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

  --accent: var(--danger);
  --accent-rgb: 226 109 90;
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
`;

/* ================= TEXT ================= */

const Title = styled.h1`
  font-size: clamp(2.2rem, 5vw, 3.2rem);
  color: var(--danger);
  text-shadow: 0 0 12px rgba(226, 109, 90, 0.5);
`;

const Lore = styled.div`
  max-width: 600px;

  display: flex;
  flex-direction: column;
  gap: 0.8rem;

  p {
    color: #9ca3af;
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

const Danger = styled.strong`
  color: var(--danger);
`;
