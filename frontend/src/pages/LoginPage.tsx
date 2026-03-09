import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import "../styles/login.css";

type AuthMode = "login" | "signup";

export default function Login() {
  const [mode, setMode] = useState<AuthMode>("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const title = useMemo(() => {
    return mode === "login" ? "SYSTEM ACCESS" : "CREATE ACCESS";
  }, [mode]);

  const subtitle = useMemo(() => {
    return mode === "login"
      ? "Authenticate to continue"
      : "Create credentials to continue";
  }, [mode]);

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    console.log({ mode, email, password });
    alert(`${mode.toUpperCase()} (frontend only)\nEmail: ${email}`);
  }

  function onProvider(provider: "google" | "facebook") {
    alert(`Continue with ${provider} (frontend only)`);
  }

  return (
    <div className="login-page">
      <div className="login-bg" aria-hidden="true" />

      <header className="login-header">
        <div className="login-header-inner">
          <a className="brand" href="/">
            <span className="brand-mark" aria-hidden="true">
              ◼
            </span>
            <span className="brand-text">EXIT.EXE</span>
          </a>
          <nav className="login-nav">
            <a className="nav-link" href="#">
              Docs
            </a>
            <a className="nav-link" href="#">
              Status
            </a>
            <a className="nav-link" href="#">
              Support
            </a>
          </nav>
        </div>
      </header>

      <main className="login-main">
        <section className="login-hero" aria-label="Authentication">
          <div className="hero-logo" aria-hidden="true">
            exit.exe
          </div>

          <p className="hero-kicker">
            AUTH REQUIRED <span className="dot">•</span> HANDSHAKE PENDING
          </p>

          <div className="auth-card">
            <div className="auth-card-glow" aria-hidden="true" />

            <div className="auth-head">
              <h1 className="auth-title">{title}</h1>
              <p className="auth-subtitle">{subtitle}</p>

              <div className="auth-mode">
                <button
                  type="button"
                  className={`mode-btn ${mode === "login" ? "active" : ""}`}
                  onClick={() => setMode("login")}
                >
                  Log in
                </button>
                <button
                  type="button"
                  className={`mode-btn ${mode === "signup" ? "active" : ""}`}
                  onClick={() => setMode("signup")}
                >
                  Sign up
                </button>
              </div>
            </div>

            <div className="provider-row">
              <button
                type="button"
                className="provider-btn"
                onClick={() => onProvider("google")}
              >
                <GoogleIcon />
                <span>Continue with Google</span>
              </button>
              <button
                type="button"
                className="provider-btn"
                onClick={() => onProvider("facebook")}
              >
                <FacebookIcon />
                <span>Continue with Facebook</span>
              </button>
            </div>

            <div className="divider">
              <span>or</span>
            </div>

            <form className="auth-form" onSubmit={onSubmit}>
              <label className="field">
                <span className="label">Email</span>
                <input
                  className="input"
                  type="email"
                  autoComplete="email"
                  placeholder="name@domain.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </label>

              <label className="field">
                <span className="label">Password</span>
                <input
                  className="input"
                  type="password"
                  autoComplete={
                    mode === "login" ? "current-password" : "new-password"
                  }
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </label>

              <div className="row">
                <label className="checkbox">
                  <input type="checkbox" />
                  <span>Remember this device</span>
                </label>
                <a className="small-link" href="#">
                  Forgot password?
                </a>
              </div>

              <button className="primary-btn" type="submit">
                {mode === "login" ? "ACCESS SYSTEM" : "CREATE ACCOUNT"}
              </button>

              <p className="fineprint">
                By continuing you agree to the{" "}
                <a href="#" className="inline-link">
                  Terms
                </a>{" "}
                and{" "}
                <a href="#" className="inline-link">
                  Privacy Policy
                </a>
                .
              </p>
            </form>
          </div>

          <p className="mystery-line">
            SESSION LOCKED. IDENTIFY YOURSELF TO ENTER THE ARCHIVE.
          </p>
        </section>
      </main>

      <footer className="login-footer">
        <div className="footer-inner">
          <span className="footer-left">EXIT.EXE</span>
          <div className="footer-center">
            <a href="#" className="footer-link">
              Privacy Policy
            </a>
            <a href="#" className="footer-link">
              Terms of Service
            </a>
          </div>
          <span className="footer-right">
            © {new Date().getFullYear()} GRID ARCHIVE
          </span>
        </div>
      </footer>
    </div>
  );
}

function GoogleIcon() {
  return (
    <svg className="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path
        d="M21.35 11.1H12v2.95h5.35c-.5 2.9-2.95 4.25-5.35 4.25A6.3 6.3 0 1 1 12 5.7c1.6 0 2.7.6 3.35 1.2l2-2A9.1 9.1 0 0 0 12 3a9 9 0 1 0 0 18c5.2 0 8.7-3.65 8.7-8.8 0-.6-.05-1.05-.15-1.1Z"
        fill="currentColor"
      />
    </svg>
  );
}

function FacebookIcon() {
  return (
    <svg className="icon" viewBox="0 0 24 24" aria-hidden="true">
      <path
        d="M13.5 22v-8h2.7l.4-3h-3.1V9.1c0-.9.25-1.5 1.55-1.5h1.65V5c-.28-.04-1.25-.12-2.38-.12-2.36 0-3.97 1.44-3.97 4.08V11H8v3h2.34v8h3.16Z"
        fill="currentColor"
      />
    </svg>
  );
}
