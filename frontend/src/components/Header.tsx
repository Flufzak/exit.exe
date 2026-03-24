import styled from "styled-components";
import { useAuth } from "../hooks/useAuth";
import { request } from "../api/request";
import AppButton from "./ui/AppButton";
import { Link, NavLink } from "react-router-dom";
import ThemeSwitch from "./ui/ThemeSwitch";
import LanguageSwitch from "./ui/LanguageSwitch";
import { useTranslation } from "react-i18next";

export default function Header() {
  const { isAuthenticated, loading, refetch } = useAuth();
  const { t } = useTranslation();

  async function handleLogout() {
    await request("/api/auth/logout", { method: "POST" });
    refetch();
  }

  if (loading) return null;
  // niet responsive
  return (
    <Container>
      <Left>
        <Link to="/">
          <LogoDark src="/images/logo/darkmodeLogo.png" alt="Exit.exe" />
          <LogoLight src="/images/logo/lightmodeLogo.png" alt="Exit.exe" />
        </Link>
      </Left>

      <Right>
        <Nav>
          <NavLink className="accent-link" to="/stories">
            {t("stories")}
          </NavLink>

          {isAuthenticated && (
            <NavLink className="accent-link" to="/profile">
              {t("profile")}
            </NavLink>
          )}
        </Nav>

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
      </Right>
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
  height: 12rem;

  html[data-theme="light"] & {
    display: none;
  }
`;

const LogoLight = styled.img`
  height: 12rem;
  display: none;

  html[data-theme="light"] & {
    display: block;
  }
`;

const Nav = styled.nav`
  display: flex;
  gap: 24px;

  a.active {
    color: var(--accent);
    text-shadow: 0 0 6px rgb(var(--accent-rgb) / 0.6);
  }

  @media (max-width: 768px) {
    display: none;
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
