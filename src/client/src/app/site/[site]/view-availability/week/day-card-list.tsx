import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import dayjs from 'dayjs';
import { DaySummaryCard } from './day-summary-card';
import { fetchPermissions } from '@services/appointmentsService';

type Props = {
  site: Site;
  ukWeekStart: dayjs.Dayjs;
  ukWeekEnd: dayjs.Dayjs;
};

export const DayCardList = async ({ site, ukWeekStart, ukWeekEnd }: Props) => {
  const [weekSummary, permissions] = await Promise.all([
    summariseWeek(ukWeekStart, ukWeekEnd, site.id),
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
