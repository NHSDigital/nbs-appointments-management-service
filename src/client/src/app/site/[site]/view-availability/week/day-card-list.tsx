import { mapWeekSummary } from '@services/availabilityCalculatorService';
import { Site, WeekSummary } from '@types';
import { DaySummaryCard } from './day-summary-card';
import {
  fetchClinicalServices,
  fetchFeatureFlag,
  fetchPermissions,
  fetchWeekSummaryV2,
} from '@services/appointmentsService';
import { RFC3339Format, DayJsType } from '@services/timeService';

type Props = {
  site: Site;
  ukWeekStart: DayJsType;
  ukWeekEnd: DayJsType;
};

const emptyWeekSummary: WeekSummary = {
  daySummaries: [],
  maximumCapacity: 0,
  bookedAppointments: 0,
  orphanedAppointments: 0,
  remainingCapacity: 0,
  startDate: {} as DayJsType,
  endDate: {} as DayJsType,
};

export const DayCardList = async ({ site, ukWeekStart, ukWeekEnd }: Props) => {
  let weekSummary: WeekSummary = emptyWeekSummary;

  const [weekSummaryV2, permissions, clinicalServices, cancelDayFlag] =
    await Promise.all([
      fetchWeekSummaryV2(site.id, ukWeekStart.format(RFC3339Format)),
      fetchPermissions(site.id),
      fetchClinicalServices(),
      fetchFeatureFlag('CancelDay'),
    ]);

  weekSummary = mapWeekSummary(ukWeekStart, ukWeekEnd, weekSummaryV2);

  const canManageAvailability = permissions.includes('availability:setup');
  const canViewDailyAppointments = permissions.includes('booking:view-detail');

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
              canViewDailyAppointments={canViewDailyAppointments}
              cancelDayFlag={cancelDayFlag.enabled}
            />
          </li>
        );
      })}
    </ol>
  );
};
