import { createGlobalStyle } from "styled-components";
import StoriesSection from "../components/stories/StoriesSection";

export default function Home() {
  return (
    <>
      <HomeStyles />
      <section className="hero">
        <h1>Escape the sect dungeons</h1>
        <p>
          Solve clever puzzles, uncover hidden clues, and find your way out. A
          thrilling escape adventure awaits.
        </p>
      </section>
      <StoriesSection />
    </>
  );
}

const HomeStyles = createGlobalStyle`
  .hero {
    position: relative;
    min-height: 420px;
    border-radius: 3px;

    display: flex;
    flex-direction:column;
    align-items: center;
    justify-content: center;
    text-align: center;

    background-image: url("/images/landingImg.png");
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;

    padding: 2rem;
    overflow: hidden;
  }

  .hero::before {
    content: "";
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.55); 
    z-index: 1;
  }

  .hero h1, p {
    position: relative;
    z-index: 2;
  }
`;
