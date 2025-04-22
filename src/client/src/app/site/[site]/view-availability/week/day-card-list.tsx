import { summariseWeek } from '@services/availabilityCalculatorService';
import { Site } from '@types';
import { DaySummaryCard } from './day-summary-card';
import {
  fetchClinicalServices,
  fetchPermissions,
} from '@services/appointmentsService';
import { DayJsType } from '@services/timeService';

type Props = {
  site: Site;
  ukWeekStart: DayJsType;
  ukWeekEnd: DayJsType;
};

export const DayCardList = async ({ site, ukWeekStart, ukWeekEnd }: Props) => {
  const [weekSummary, permissions, clinicalServices] = await Promise.all([
    summariseWeek(ukWeekStart, ukWeekEnd, site.id),
    fetchPermissions(site.id),
    fetchClinicalServices(),
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
              clinicalServices={clinicalServices}
            />
          </li>
        );
      })}
    </ol>
  );
};
