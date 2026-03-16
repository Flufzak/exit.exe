import styled from "styled-components";
import { useAuth } from "../hooks/useAuth";
import { request } from "../api/request";
import AppButton from "./ui/AppButton";
import { t } from "i18next";
import { Link, NavLink } from "react-router-dom";
import ThemeSwitch from "./ui/ThemeSwitch";

export default function Header() {
  const { isAuthenticated, loading, refetch } = useAuth();

  async function handleLogout() {
    await request("/api/auth/logout", { method: "POST" });
    refetch();
  }

  if (loading) return null;

  return (
    <Container>
      <Left>
        <Link to="/">
          <Logo src="/images/logo/darkmodeLogo.png" alt="Exit.exe" />
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

const Logo = styled.img`
  height: 12rem;
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
