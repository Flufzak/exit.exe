import styled from "styled-components";
import { useTranslation } from "react-i18next";

export default function LanguageSwitch() {
  const { i18n } = useTranslation();
  const currentLang = i18n.language;

  function toggleLanguage() {
    const newLang = currentLang === "nl" ? "en" : "nl";
    i18n.changeLanguage(newLang);
    localStorage.setItem("lang", newLang);
  }

  return (
    <Switch onClick={toggleLanguage}>
      {currentLang === "nl" ? "NL" : "EN"}
    </Switch>
  );
}

const Switch = styled.button`
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text-primary);
  padding: 6px 10px;
  border-radius: 3px;
  cursor: pointer;
  font-size: 0.8rem;

  &:hover {
    border-color: var(--accent);
    color: var(--accent);
    box-shadow: 0 0 6px rgb(var(--accent-rgb) / 0.5);
  }
`;
