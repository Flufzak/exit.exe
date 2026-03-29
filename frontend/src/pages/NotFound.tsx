import styled, { keyframes } from "styled-components";
import AppButton from "../components/ui/AppButton";
import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

export default function NotFound() {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <Wrapper>
      <Particles />

      <Content>
        <Code>404</Code>

        <Title>{t("should-not-come")}</Title>

        <Text>{t("lost-story")}</Text>

        <Muted>{t("safe-grounds")}</Muted>

        <Buttons>
          <AppButton onClick={() => navigate("/")}>
            {t("return-home")}
          </AppButton>

          <AppButton variant="secondary" onClick={() => navigate("/stories")}>
            {t("start-adventure")}
          </AppButton>
        </Buttons>
      </Content>
    </Wrapper>
  );
}

/* ================= ANIMATIONS ================= */

const glow = keyframes`
  0% { text-shadow: 0 0 4px rgba(var(--accent-rgb), 0.2); }
  50% { text-shadow: 0 0 18px rgba(var(--accent-rgb), 0.7); }
  100% { text-shadow: 0 0 4px rgba(var(--accent-rgb), 0.2); }
`;

const float = keyframes`
  0% { transform: translateY(0); opacity: 0.2; }
  50% { transform: translateY(-40px); opacity: 0.8; }
  100% { transform: translateY(0); opacity: 0.2; }
`;

/* ================= LAYOUT ================= */

const Wrapper = styled.div`
  position: fixed; /* 💥 KEY FIX */
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden; /* 💥 NO SCROLL */
  background: var(--bg);
`;

const Content = styled.div`
  text-align: center;
  max-width: 650px;
  padding: 1rem;
  z-index: 2;
`;

/* ================= TEXT ================= */

const Code = styled.div`
  font-size: clamp(3rem, 10vw, 6rem);
  font-weight: 700;
  animation: ${glow} 3s infinite ease-in-out;
  margin-bottom: 1rem;
`;

const Title = styled.h1`
  font-size: clamp(1.5rem, 4vw, 2.5rem);
  margin-bottom: 1rem;
`;

const Text = styled.p`
  font-size: clamp(0.95rem, 2vw, 1.1rem);
  margin-bottom: 0.5rem;
`;

const Muted = styled.p`
  color: var(--text-secondary);
  font-size: 0.95rem;
`;

/* ================= BUTTONS ================= */

const Buttons = styled.div`
  margin-top: 2rem;
  display: flex;
  gap: 1rem;
  justify-content: center;
`;

/* ================= PARTICLES ================= */

const Particle = styled.div`
  position: absolute;
  width: 12px;
  height: 12px;

  background: radial-gradient(
    circle,
    rgba(110, 231, 183, 1) 0%,
    rgba(110, 231, 183, 0.4) 40%,
    transparent 100%
  );

  border-radius: 50%;

  animation: ${float} 6s ease-in-out infinite;

  filter: blur(1px) drop-shadow(0 0 6px rgba(110, 231, 183, 0.8));
`;

type ParticleType = {
  x: number;
  y: number;
  weight: number;
  lag: number;
};

const Particles = () => {
  const [mouse, setMouse] = useState({ x: 50, y: 50 });

  const [particles] = useState<ParticleType[]>(
    Array.from({ length: 30 }).map(() => ({
      x: Math.random() * 100,
      y: Math.random() * 100,
      weight: 0.02 + Math.random() * 0.08,
      lag: 0.3 + Math.random() * 0.6,
    })),
  );

  useEffect(() => {
    const handleMove = (e: MouseEvent) => {
      setMouse({
        x: (e.clientX / window.innerWidth) * 100,
        y: (e.clientY / window.innerHeight) * 100,
      });
    };

    window.addEventListener("mousemove", handleMove);
    return () => window.removeEventListener("mousemove", handleMove);
  }, []);

  return (
    <>
      {particles.map((p, i) => {
        const dx = mouse.x - p.x;
        const dy = mouse.y - p.y;

        return (
          <Particle
            key={i}
            style={{
              left: `${p.x + dx * p.weight}%`,
              top: `${p.y + dy * p.weight}%`,
              transition: `left ${p.lag}s ease-out, top ${p.lag}s ease-out`,
            }}
          />
        );
      })}
    </>
  );
};
