import styled from "styled-components";
import { Story } from "../../types/story";
import { useTranslation } from "react-i18next";

type Props = Story;

const storyImages: Record<string, string> = {
  kazimir: "/images/landingImg.png",
  abyss: "/images/nightfall.png",
};

export default function StoryCard({ id, duration, status, type }: Props) {
  const imageUrl = storyImages[id];
  const { t } = useTranslation();

  return (
    <Card $type={type}>
      <Image style={{ backgroundImage: `url(${imageUrl})` }} />

      <Content>
        <Tag>
          {type === "upcoming" ? t("tag-new-chapter") : t("tag-featured-story")}
        </Tag>

        <Title>{t(`story-${id}-title`)}</Title>
        <Description>{t(`story-${id}-description`)}</Description>

        {type === "available" && (
          <Meta>
            {duration && <span>{duration}</span>}
            {status && (
              <Status>
                {t(`status-${status.toLowerCase().replace(" ", "-")}`)}
              </Status>
            )}
          </Meta>
        )}

        {type === "upcoming" && (
          <LockedOverlay>{t("status-coming-soon")}</LockedOverlay>
        )}
      </Content>
    </Card>
  );
}

const Card = styled.div<{ $type: "available" | "upcoming" }>`
  display: flex;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 3px;
  overflow: hidden;
  position: relative;

  ${({ $type }) =>
    $type === "upcoming" &&
    `
      opacity: 0.85;
      filter: grayscale(70%);
    `}
  @media (max-width: 768px) {
    flex-direction: column;
  }
`;

const Image = styled.div`
  flex: 0 0 240px;
  background-size: cover;
  background-position: center;
`;

const Content = styled.div`
  padding: 1.25rem;
  flex: 1;
  position: relative;
`;

const Tag = styled.div`
  font-size: 0.8rem;
  color: var(--accent);
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const Title = styled.h2`
  margin: 0 0 1rem 0;
  color: var(--text-primary);
`;

const Description = styled.p`
  color: var(--text-secondary);
`;

const Meta = styled.div`
  margin-top: 1.5rem;
  display: flex;
  gap: 1.5rem;
  font-size: 0.9rem;
  color: var(--text-secondary);
`;

const Status = styled.span`
  color: var(--accent);
`;

const LockedOverlay = styled.div`
  margin-top: 1.5rem;
  font-size: 0.9rem;
  color: var(--text-secondary);
`;
