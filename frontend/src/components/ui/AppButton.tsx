import React from "react";

type AppButtonProps = {
  children: React.ReactNode;
  variant?: "primary" | "secondary";
  onClick?: () => void;
  type?: "button" | "submit";
};

export default function AppButton({
  children,
  variant = "primary",
  onClick,
  type = "button",
}: AppButtonProps) {
  return (
    <>
      <style>
        {`
        .app-button {
          position: relative;
          overflow: hidden;

          display: inline-flex;
          align-items: center;
          justify-content: center;

          padding: 0.7rem 1.4rem;
          border-radius: 10px;

          font-size: 0.95rem;
          font-weight: 600;
          letter-spacing: 0.02em;

          border: 1px solid var(--border);
          cursor: pointer;

          transition:
            transform 0.12s ease,
            background 0.2s ease,
            border-color 0.2s ease,
            box-shadow 0.2s ease,
            filter 0.2s ease;
        }

        .app-button span {
          position: relative;
          z-index: 2;
        }

        /* sweep shine animation */
        .app-button::before {
          content: "";
          position: absolute;
          inset: 0;

          background: linear-gradient(
            120deg,
            transparent 30%,
            rgba(255 255 255 / 0.25),
            transparent 70%
          );

          transform: translateX(-120%);
          transition: transform 0.6s ease;
        }

        .app-button:hover::before {
          transform: translateX(120%);
        }

        /* shared hover behaviour */
        .app-button:hover {
          transform: translateY(-1px);

          box-shadow:
            0 4px 10px rgba(0 0 0 / 0.35),
            0 0 10px rgba(var(--accent-rgb) / 0.25);
        }

        /* click feedback */
        .app-button:active {
          transform: translateY(0) scale(0.96);
        }

        /* PRIMARY */

        .app-button-primary {
          background: var(--accent);
          color: #02140e;
          border-color: transparent;
        }

        .app-button-primary:hover {
          filter: brightness(1.08);
        }

        /* SECONDARY */

        .app-button-secondary {
          background: var(--surface);
          color: var(--text-primary);
        }

        .app-button-secondary:hover {
          border-color: var(--accent);
        }
        `}
      </style>

      <button
        className={`app-button app-button-${variant}`}
        onClick={onClick}
        type={type}
      >
        <span>{children}</span>
      </button>
    </>
  );
}
