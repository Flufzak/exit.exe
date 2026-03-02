import styled from "styled-components";
import { Story } from "../../types/story";

type Props = Story;

const storyImages: Record<string, string> = {
  kazimir: "/images/landingImg.png",
  abyss: "/images/nightfall.png",
};

export default function StoryCard({
  id,
  title,
  description,
  duration,
  status,
  type,
}: Props) {
  const imageUrl = storyImages[id];

  return (
    <Card $type={type}>
      <Image style={{ backgroundImage: `url(${imageUrl})` }} />

      <Content>
        <Tag>{type === "upcoming" ? "New Chapter" : "Featured Story"}</Tag>

        <Title>{title}</Title>
        <Description>{description}</Description>

        {type === "available" && (
          <Meta>
            {duration && <span>{duration}</span>}
            {status && <Status>{status}</Status>}
          </Meta>
        )}

        {type === "upcoming" && <LockedOverlay>Coming Soon</LockedOverlay>}
      </Content>
    </Card>
  );
}

const Card = styled.div<{ $type: "available" | "upcoming" }>`
  display: flex;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  overflow: hidden;
  position: relative;

  ${({ $type }) =>
    $type === "upcoming" &&
    `
      opacity: 0.85;
      filter: grayscale(70%);
    `}
`;

const Image = styled.div`
  width: 300px;
  background-size: cover;
  background-position: center;
`;

const Content = styled.div`
  padding: 2rem;
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
  max-width: 600px;
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
