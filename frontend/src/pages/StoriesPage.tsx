import StoriesSection from "../components/stories/StoriesSection";
import { t } from "i18next";

export default function StoriesPage() {
  return (
    <div>
      <h1>{t("all-stories")}</h1>
      <StoriesSection />
    </div>
  );
}
