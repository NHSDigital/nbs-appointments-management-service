import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import dayjs from 'dayjs';
import { WeekSummaryCard } from './week-summary-card';

type Props = {
  site: Site;
  ukWeeks: dayjs.Dayjs[][];
};

export const WeekCardList = async ({ site, ukWeeks }: Props) => {
  const ukWeekSummaries = await Promise.all(
    ukWeeks.map(async week => {
      return summariseWeek(week[0], week[6], site.id);
    }),
  );

  return (
    <ol className="card-list">
      {ukWeekSummaries.map((week, weekIndex) => {
        return (
          <li key={`week-summary-${weekIndex}`}>
            <WeekSummaryCard ukWeekSummary={week} />
          </li>
        );
      })}
    </ol>
  );
};
