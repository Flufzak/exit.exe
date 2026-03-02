import { useEffect, useState } from "react";
import { request } from "../api/request";
import { Story } from "../types/story";

export function useStories() {
  const [stories, setStories] = useState<Story[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function fetchStories() {
    setLoading(true);
    setError(null);

    try {
      const data = await request<Story[]>("/api/stories");
      setStories(data);
    } catch {
      setError("Failed to load stories. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchStories();
  }, []);

  return { stories, loading, error, refetch: fetchStories };
}
