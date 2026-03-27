import { useTranslation } from "react-i18next";
import StoriesSection from "../components/stories/StoriesSection";

export default function StoriesPage() {
  const { t } = useTranslation();

  return (
    <div>
      <h1>{t("all-stories")}</h1>
      <hr />
      <StoriesSection />
    </div>
  );
}
