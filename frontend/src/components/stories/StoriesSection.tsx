import styled from "styled-components";
import { Link, useLocation } from "react-router-dom";
import StoryCard from "./StoryCard";
import { useStories } from "../../hooks/useStories";
import Loader from "../ui/Loader";

export default function StoriesSection() {
  const { stories, loading, error } = useStories();
  const location = useLocation();

  const isHome = location.pathname === "/";

  if (loading) return <Loader />;
  if (error) return <p>{error}</p>;

  const availableStories = stories
    .filter((story) => story.type === "available")
    .slice(0, isHome ? 3 : undefined);

  const upcomingStories = stories.filter((story) => story.type === "upcoming");

  return (
    <Section>
      <Header>
        <h2>Available Stories</h2>

        {isHome && (
          <ViewAll to="/stories" className="accent-link">
            View All
          </ViewAll>
        )}
      </Header>

      <Cards>
        {availableStories.map((story) => (
          <StoryCard key={story.id} {...story} />
        ))}
      </Cards>

      {upcomingStories.length > 0 && (
        <>
          <SubHeader>
            <h2>Upcoming</h2>
          </SubHeader>

          <Cards>
            {upcomingStories.map((story) => (
              <StoryCard key={story.id} {...story} />
            ))}
          </Cards>
        </>
      )}
    </Section>
  );
}

const Section = styled.section`
  margin-top: 3rem;
`;

const Header = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.8rem;
  color: var(--text-primary);
`;

const SubHeader = styled.div`
  margin-top: 2rem;
  margin-bottom: 0.8rem;
  color: var(--text-primary);
`;

const Cards = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
`;

const ViewAll = styled(Link)`
  font-size: 0.9rem;
  cursor: pointer;
`;
