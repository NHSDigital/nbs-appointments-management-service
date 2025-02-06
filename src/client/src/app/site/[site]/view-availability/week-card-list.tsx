import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import dayjs from 'dayjs';
import { WeekSummaryCard } from './week-summary-card';

type Props = {
  site: Site;
  weeks: dayjs.Dayjs[][];
};

export const WeekCardList = async ({ site, weeks }: Props) => {
  const weekSummaries = await Promise.all(
    weeks.map(async week => {
      return summariseWeek(week[0], week[6], site.id);
    }),
  );

  return (
    <>
      {weekSummaries.map((week, weekIndex) => {
        return (
          <WeekSummaryCard
            weekSummary={week}
            key={`week-summary-${weekIndex}`}
          />
        );
      })}
    </>
  );
};
