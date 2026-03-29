const API_URL = import.meta.env.VITE_BACKEND_URL;

export function googleLoginUrl(returnUrl?: string): string {
  const base = `${API_URL}/api/auth/login/google`;
  const url = returnUrl ?? window.location.href;
  return `${base}?returnUrl=${encodeURIComponent(url)}`;
}

export function facebookLoginUrl(returnUrl?: string): string {
  const base = `${API_URL}/api/auth/login/facebook`;
  const url = returnUrl ?? window.location.href;
  return `${base}?returnUrl=${encodeURIComponent(url)}`;
}
