import styled from "styled-components";
import { useProfileStats } from "../hooks/useProfileStats";
import { useAuth } from "../hooks/useAuth";
import { useTranslation } from "react-i18next";
import Loader from "../components/ui/Loader";
import Error from "../components/ui/Error";

export default function ProfilePage() {
  const { stats, loading, error, refetch } = useProfileStats();
  const { user } = useAuth();
  const { t } = useTranslation();

  if (loading) return <Loader />;
  if (error) return <Error message={error} onReload={refetch} />;
  if (!stats) return null;

  const winrate = stats.totalGamesPlayed
    ? Math.round((stats.gamesWon / stats.totalGamesPlayed) * 100)
    : 0;

  return (
    <Wrapper>
      <TopRow>
        <Email>{user?.email}</Email>
      </TopRow>

      <Title>{t("your-activity")}</Title>

      <Grid>
        <Card>
          <Label>{t("total-games")}</Label>
          <Value>{stats.totalGamesPlayed}</Value>
        </Card>

        <Card>
          <Label>{t("won")}</Label>
          <Value className="success">{stats.gamesWon}</Value>
        </Card>

        <Card>
          <Label>{t("lost")}</Label>
          <Value className="danger">{stats.gamesLost}</Value>
        </Card>

        <Card>
          <Label>{t("in-progress")}</Label>
          <Value>{stats.gamesInProgress}</Value>
        </Card>

        <Card>
          <Label>{t("win-rate")}</Label>
          <Value
            className={
              winrate > 70 ? "success" : winrate < 40 ? "danger" : undefined
            }
          >
            {winrate}%
          </Value>
        </Card>

        <Card>
          <Label>{t("best-score")}</Label>
          <Value>{stats.bestScore ?? "-"}</Value>
        </Card>

        <Card>
          <Label>{t("average-score")}</Label>
          <Value>
            {stats.averageScore !== null
              ? `${Math.round(stats.averageScore)}%`
              : "-"}
          </Value>
        </Card>

        <Card>
          <Label>{t("hints-used")}</Label>
          <Value className="danger">{stats.totalHintsUsed}</Value>
        </Card>

        <Card>
          <Label>{t("last-played")}</Label>
          <Value>
            {stats.lastPlayedAtUtc
              ? new Date(stats.lastPlayedAtUtc).toLocaleDateString()
              : "-"}
          </Value>
        </Card>
      </Grid>
    </Wrapper>
  );
}

const Wrapper = styled.div`
  width: 100%;
`;

const TopRow = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
`;

const Email = styled.p`
  font-size: 0.85rem;
  color: var(--text-secondary);
`;

const Title = styled.h1`
  margin-bottom: 1.5rem;
`;

const Grid = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;

  @media (max-width: 900px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 500px) {
    grid-template-columns: 1fr;
  }
`;

const Card = styled.div`
  padding: 1.4rem;
  border: 1px solid var(--border);
  border-radius: 3px;
  background: var(--surface);
`;

const Label = styled.p`
  color: var(--text-secondary);
`;

const Value = styled.h2`
  margin-top: 0.3rem;
  color: var(--text-primary);

  &.success {
    color: var(--accent);
  }

  &.danger {
    color: var(--danger);
  }
`;
