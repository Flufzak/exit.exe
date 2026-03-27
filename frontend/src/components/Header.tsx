import styled from "styled-components";
import { useAuth } from "../hooks/useAuth";
import { request } from "../api/request";
import AppButton from "./ui/AppButton";
import { Link, NavLink } from "react-router-dom";
import ThemeSwitch from "./ui/ThemeSwitch";
import LanguageSwitch from "./ui/LanguageSwitch";
import { useTranslation } from "react-i18next";
import { useState, useEffect } from "react";
import HamburgerMenu from "./ui/HamburgerMenu";

export default function Header() {
  const { isAuthenticated, loading, refetch } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const { t } = useTranslation();

  useEffect(() => {
    const onResize = () => setIsMobile(window.innerWidth <= 768);
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  async function handleLogout() {
    await request("/api/auth/logout", { method: "POST" });
    refetch();
  }

  const NavLinks = (
    <NavGroup>
      <NavLink className="accent-link" to="/stories">
        {t("stories")}
      </NavLink>

      {isAuthenticated && (
        <NavLink className="accent-link" to="/profile">
          {t("profile")}
        </NavLink>
      )}
    </NavGroup>
  );

  if (loading) return null;

  return (
    <Container>
      <Left>
        <Link to="/">
          <LogoDark src="/images/logo/darkmodeLogoTr.png" alt="Exit.exe" />
          <LogoLight src="/images/logo/lightmodeLogoTr.png" alt="Exit.exe" />
        </Link>
      </Left>

      <Right>
        {/* Desktop */}
        {!isMobile && (
          <DesktopOnly>
            <div>{NavLinks}</div>

            <Actions>
              <ThemeSwitch />
              <LanguageSwitch />
            </Actions>

            {!isAuthenticated && (
              <NavLink to="/login">
                <AppButton>{t("log-in")}</AppButton>
              </NavLink>
            )}

            {isAuthenticated && (
              <AppButton onClick={handleLogout}>{t("log-out")}</AppButton>
            )}
          </DesktopOnly>
        )}

        {/* Mobile */}
        {isMobile && (
          <MobileOnly>
            <HamburgerMenu
              open={menuOpen}
              onToggle={() => setMenuOpen(!menuOpen)}
            />
          </MobileOnly>
        )}
      </Right>

      {isMobile && menuOpen && (
        <MobileMenu>
          <div onClick={() => setMenuOpen(false)}>{NavLinks}</div>
          <Divider />
          <ThemeSwitch />
          <LanguageSwitch />
          <Divider />
          {!isAuthenticated && (
            <NavLink to="/login" onClick={() => setMenuOpen(false)}>
              <AppButton>{t("log-in")}</AppButton>
            </NavLink>
          )}
          {isAuthenticated && (
            <AppButton
              onClick={() => {
                handleLogout();
                setMenuOpen(false);
              }}
            >
              {t("log-out")}
            </AppButton>
          )}{" "}
        </MobileMenu>
      )}
    </Container>
  );
}

const Container = styled.header`
  height: 50px;
  padding: 0 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--bg);
  border-bottom: 1px solid var(--border);

  @media (max-width: 768px) {
    padding: 0 16px;
  }
`;

const Left = styled.div`
  display: flex;
  align-items: center;
  gap: 32px;
`;

const LogoDark = styled.img`
  height: 60px;
  width: auto;
  object-fit: contain;

  html[data-theme="light"] & {
    display: none;
  }
`;

const LogoLight = styled.img`
  height: 60px;
  width: auto;
  object-fit: contain;

  display: none;

  html[data-theme="light"] & {
    display: block;
  }
`;

const Right = styled.div`
  display: flex;
  align-items: center;
  gap: 32px;
`;

const Actions = styled.div`
  display: flex;
  align-items: center;
  gap: 16px;
`;

/* 👇 NIEUW maar raakt je bestaande styling niet */
const DesktopOnly = styled.div`
  display: flex;
  align-items: center;
  gap: 32px;

  @media (max-width: 768px) {
    display: none;
  }
`;

const MobileOnly = styled.div`
  display: none;

  @media (max-width: 768px) {
    display: block;
  }
`;

const MobileMenu = styled.div`
  position: absolute;
  top: 60px;
  right: 16px;

  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;

  padding: 16px;

  display: flex;
  flex-direction: column;
  gap: 16px;

  min-width: 200px;
  max-width: 240px;

  z-index: 999;

  box-shadow:
    0 10px 30px rgba(0, 0, 0, 0.4),
    0 0 10px rgba(var(--accent-rgb) / 0.1);

  a {
    display: flex;
    text-decoration: none;
    color: var(--text-primary);
  }
  a > * {
    flex: 1;
  }
`;

const Divider = styled.div`
  height: 1px;
  background: var(--border);
  margin: 8px 0;
`;

const NavGroup = styled.div`
  display: flex;
  gap: 24px;

  @media (max-width: 768px) {
    flex-direction: column;
    gap: 12px;
  }

  a.active {
    color: var(--accent);
    text-shadow: 0 0 6px rgb(var(--accent-rgb) / 0.6);
  }
`;
