import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import dayjs from 'dayjs';
import { DaySummaryCard } from './day-summary-card';
import { fetchPermissions } from '@services/appointmentsService';

type Props = {
  site: Site;
  weekStart: dayjs.Dayjs;
  weekEnd: dayjs.Dayjs;
};

export const DayCardList = async ({ site, weekStart, weekEnd }: Props) => {
  const [weekSummary, permissions] = await Promise.all([
    summariseWeek(weekStart, weekEnd, site.id),
    fetchPermissions(site.id),
  ]);

  const canManageAvailability = permissions.includes('availability:setup');

  return (
    <ol className="card-list">
      {weekSummary.daySummaries.map((day, dayIndex) => {
        return (
          <li key={`day-summary-${dayIndex}`}>
            <DaySummaryCard
              daySummary={day}
              siteId={site.id}
              canManageAvailability={canManageAvailability}
            />
          </li>
        );
      })}
    </ol>
  );
};
