import { useTranslation } from "react-i18next";
import styled from "styled-components";

interface ErrorProps {
  message: string;
  onReload?: () => void;
}

export default function Error({ message, onReload }: ErrorProps) {
  const { t } = useTranslation();

  return (
    <Wrapper>
      <Box>
        <Title>{t("error")}</Title>
        <Message>{message}</Message>

        <ReloadButton onClick={onReload}>{t("retry")}</ReloadButton>
      </Box>
    </Wrapper>
  );
}

const Wrapper = styled.div`
  padding: 3rem 0;
`;

const Box = styled.div`
  background: var(--surface);
  border: 1px solid var(--danger);
  border-radius: 3px;
  padding: 24px;
`;

const Title = styled.h2`
  color: var(--danger);
  margin-bottom: 0.8rem;
  font-weight: 700;
`;

const Message = styled.p`
  color: var(--text-primary);
  margin-bottom: 1.5rem;
`;

const ReloadButton = styled.button`
  background: transparent;
  border: 1px solid var(--danger);
  color: var(--danger);
  padding: 0.6rem 1.2rem;
  border-radius: 3px;
  cursor: pointer;
  font-family: inherit;

  transition: all 0.2s ease;

  &:hover {
    background: var(--danger);
    color: var(--bg);
  }
`;
