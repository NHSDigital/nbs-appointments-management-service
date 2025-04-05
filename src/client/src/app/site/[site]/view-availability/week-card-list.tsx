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
    ukWeeks.map(async ukWeek => {
      return summariseWeek(ukWeek[0], ukWeek[6], site.id);
    }),
  );

  return (
    <>
      {ukWeekSummaries.map((week, weekIndex) => {
        return (
          <WeekSummaryCard
            ukWeekSummary={week}
            key={`week-summary-${weekIndex}`}
          />
        );
      })}
    </>
  );
};
