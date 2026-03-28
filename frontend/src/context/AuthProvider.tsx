import { useEffect, useState } from "react";
import { request } from "../api/request";
import { MeResponse } from "../types/auth";
import { authRoutes } from "../api/routes";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: React.ReactNode }) {
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

  async function logout() {
    try {
      await request("/api/auth/logout", { method: "POST" });
    } finally {
      setUser(null);
    }
  }

  function loginWithGoogle() {
    window.location.href = authRoutes.google;
  }

  function loginWithFacebook() {
    window.location.href = authRoutes.facebook;
  }

  useEffect(() => {
    fetchMe();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        isAuthenticated: user?.isAuthenticated ?? false,
        refetch: fetchMe,
        loginWithGoogle,
        loginWithFacebook,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
