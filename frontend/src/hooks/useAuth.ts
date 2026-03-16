import { useEffect, useState } from "react";
import { request } from "../api/request";
import { MeResponse } from "../types/auth";

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

  return {
    user,
    loading,
    isAuthenticated: user?.isAuthenticated ?? false,
    refetch: fetchMe,
  };
}
