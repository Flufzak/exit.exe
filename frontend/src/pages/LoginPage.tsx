import "../styles/login.css";
import AppButton from "../components/ui/AppButton";

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

export default function Login() {
  function onProvider(provider: "google" | "facebook") {
    alert(`Continue with ${provider} (frontend only)`);
  }

  return (
    <div className="login-page">
      <div className="login-bg" aria-hidden="true" />

      <main className="login-main">
        <section className="login-hero" aria-label="Authentication">
          <div className="hero-logo" aria-hidden="true">
            exit.exe
          </div>

          <div className="auth-card">
            <div className="auth-card-glow" aria-hidden="true" />

            <div className="provider-row">
              <AppButton variant="primary" onClick={() => onProvider("google")}>
                <GoogleIcon /> Continue with Google
              </AppButton>
              <p></p>
              <AppButton
                variant="primary"
                onClick={() => onProvider("facebook")}
              >
                <FacebookIcon /> Continue with Facebook
              </AppButton>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}
