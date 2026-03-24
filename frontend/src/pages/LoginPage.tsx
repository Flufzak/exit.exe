import { useEffect, useState } from "react";
import "../styles/login.css";
import AppButton from "../components/ui/AppButton";
import { useAuth } from "../hooks/useAuth";
import Loader from "../components/ui/Loader";

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
  const [logoSrc, setLogoSrc] = useState("/images/logo/darkmodeLogo.png");
  const { loginWithGoogle, loginWithFacebook, loading } = useAuth();

  useEffect(() => {
    const root = document.documentElement;

    const updateLogo = () => {
      const theme = root.getAttribute("data-theme");
      const isLight = theme === "light";

      setLogoSrc(
        isLight
          ? "/images/logo/lightmodeLogo.png"
          : "/images/logo/darkmodeLogo.png",
      );
    };

    updateLogo();

    const observer = new MutationObserver(() => {
      updateLogo();
    });

    observer.observe(root, {
      attributes: true,
      attributeFilter: ["data-theme"],
    });

    return () => observer.disconnect();
  }, []);

  if (loading) return <Loader />;

  return (
    <main className="login-main">
      <section className="login-hero" aria-label="Authentication">
        <div className="hero-logo-wrap" aria-hidden="true">
          <img src={logoSrc} alt="" className="hero-logo-image" />
        </div>

        <div className="auth-card">
          <div aria-hidden="true" />

          <div className="auth-head">
            <h1 className="auth-title">Log in</h1>
          </div>

          <div className="provider-row">
            <AppButton variant="primary" onClick={loginWithGoogle}>
              <span className="button-content">
                <GoogleIcon />
                <span>Continue with Google</span>
              </span>
            </AppButton>

            <AppButton variant="primary" onClick={loginWithFacebook}>
              <span className="button-content">
                <FacebookIcon />
                <span>Continue with Facebook</span>
              </span>
            </AppButton>
          </div>
        </div>
      </section>
    </main>
  );
}
