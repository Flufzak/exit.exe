import { useEffect, useState } from "react";
import { request } from "../api/request";
import { Story } from "../types/story";

export function useStories() {
  const [stories, setStories] = useState<Story[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchStories() {
      try {
        const data = await request<Story[]>("/api/stories");
        setStories(data);
      } catch (err) {
        setError(err + "Failed to load stories");
      } finally {
        setLoading(false);
      }
    }

    fetchStories();
  }, []);

  return { stories, loading, error };
}
