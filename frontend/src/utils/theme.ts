export function getStoredTheme() {
  return localStorage.getItem("theme") || "dark";
}

export function applyTheme(theme: string) {
  document.documentElement.setAttribute("data-theme", theme);
}

export function toggleTheme(current: string) {
  const next = current === "dark" ? "light" : "dark";

  localStorage.setItem("theme", next);
  applyTheme(next);

  return next;
}
