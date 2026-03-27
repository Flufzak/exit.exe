import { useEffect, useState } from "react";
import { request } from "../api/request";
import { MeResponse } from "../types/auth";
import { authRoutes } from "../api/routes";

export function useAuth() {
  const [user, setUser] = useState<MeResponse | null>(null);
  const [loading, setLoading] = useState(true);

  async function fetchMe() {
    try {
      const data = await request<MeResponse>("/api/auth/me");
      setUser(data);
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchMe();
  }, []);

  function loginWithGoogle() {
    window.location.href = authRoutes.google;
  }

  function loginWithFacebook() {
    window.location.href = authRoutes.facebook;
  }

  return {
    user,
    loading,
    isAuthenticated: user?.isAuthenticated ?? false,
    refetch: fetchMe,
    loginWithGoogle,
    loginWithFacebook,
  };
}
