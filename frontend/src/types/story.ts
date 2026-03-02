export type StoryType = "available" | "upcoming";

export interface Story {
  id: string;
  title: string;
  description: string;
  duration?: string;
  difficulty?: string;
  status?: string;
  type: StoryType;
}
