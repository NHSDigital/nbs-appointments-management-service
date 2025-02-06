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
    <>
      {weekSummary.daySummaries.map((day, dayIndex) => {
        return (
          <DaySummaryCard
            daySummary={day}
            siteId={site.id}
            key={`day-summary-${dayIndex}`}
            canManageAvailability={canManageAvailability}
          />
        );
      })}
    </>
  );
};
