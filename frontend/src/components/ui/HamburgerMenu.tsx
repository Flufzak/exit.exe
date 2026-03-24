import styled from "styled-components";

type Props = {
  open: boolean;
  onToggle: () => void;
};

export default function HamburgerMenu({ open, onToggle }: Props) {
  return (
    <Button
      onClick={onToggle}
      $open={open}
      aria-label="Toggle navigation menu"
      aria-expanded={open}
    >
      <span />
      <span />
      <span />
    </Button>
  );
}
const Button = styled.button<{ $open: boolean }>`
  display: none;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;

  @media (max-width: 768px) {
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 4px;
  }

  span {
    display: block;
    width: 18px;
    height: 2px;
    background: var(--accent);
    border-radius: 2px;

    transition:
      transform 0.25s ease,
      opacity 0.25s ease;
  }

  ${({ $open }) =>
    $open &&
    `
    span:nth-child(1) {
      transform: translateY(6px) rotate(45deg);
    }
    span:nth-child(2) {
      opacity: 0;
    }
    span:nth-child(3) {
      transform: translateY(-6px) rotate(-45deg);
    }
  `}
`;
