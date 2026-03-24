import { useEffect, useState } from "react";
import { request } from "../api/request";
import { Story } from "../types/story";
import { useTranslation } from "react-i18next";

export function useStories() {
  const [stories, setStories] = useState<Story[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { t } = useTranslation();

  async function fetchStories() {
    setLoading(true);
    setError(null);

    try {
      const data = await request<Story[]>("/api/stories");
      setStories(data);
    } catch {
      setError(t("stories-error", { action: t("try-again") }));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchStories();
  }, []);

  return { stories, loading, error, refetch: fetchStories };
}
