import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import { WeekSummaryCard } from './week-summary-card';
import { fetchClinicalServices } from '@services/appointmentsService';
import { DayJsType } from '@services/timeService';

type Props = {
  site: Site;
  ukWeeks: DayJsType[][];
};

export const WeekCardList = async ({ site, ukWeeks }: Props) => {
  const ukWeekSummaries = await Promise.all(
    ukWeeks.map(async week => {
      return summariseWeek(week[0], week[6], site.id);
    }),
  );

  const clinicalServices = await fetchClinicalServices();

  return (
    <ol className="card-list">
      {ukWeekSummaries.map((week, weekIndex) => {
        return (
          <li key={`week-summary-${weekIndex}`}>
            <WeekSummaryCard
              ukWeekSummary={week}
              clinicalServices={clinicalServices}
            />
          </li>
        );
      })}
    </ol>
  );
};
