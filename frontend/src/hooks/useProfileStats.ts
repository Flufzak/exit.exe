import { useEffect, useState } from "react";
import { request } from "../api/request";
import { ProfileStats } from "../types/profile";
import { useTranslation } from "react-i18next";

export function useProfileStats() {
  const [stats, setStats] = useState<ProfileStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { t } = useTranslation();

  async function fetchStats() {
    setLoading(true);
    setError(null);

    try {
      const data = await request<ProfileStats>("/api/profile/stats");
      setStats(data);
    } catch {
      setError(t("profile-error", { action: t("try-again") }));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchStats();
    // eslint-disable-next-line
  }, []);

  return { stats, loading, error, refetch: fetchStats };
}
