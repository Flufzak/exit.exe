import { useState } from "react";

const API_BASE_URL = import.meta.env.VITE_BACKEND_URL;

export function useAuth() {
  const [loading, setLoading] = useState(false);

  function loginWithGoogle() {
    setLoading(true);
    window.location.href = `${API_BASE_URL}/api/auth/login/google`;
  }

  return { loginWithGoogle, loading };
}
