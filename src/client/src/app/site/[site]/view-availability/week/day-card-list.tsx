import {
  mapWeekSummary,
  summariseWeek,
} from '@services/availabilityCalculatorService';
import { ClinicalService, Site, WeekSummary } from '@types';
import { DaySummaryCard } from './day-summary-card';
import {
  fetchClinicalServices,
  fetchFeatureFlag,
  fetchPermissions,
  fetchWeekSummary,
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
  let permissions: string[];
  let clinicalServices: ClinicalService[];

  const [multipleServicesFlag, cancelDayFlag] = await Promise.all([
    fetchFeatureFlag('MultipleServices'),
    fetchFeatureFlag('CancelDay'),
  ]);

  if (multipleServicesFlag.enabled) {
    const [weekSummaryV2, permissionsV2, clinicalServicesV2] =
      await Promise.all([
        fetchWeekSummaryV2(site.id, ukWeekStart.format(RFC3339Format)),
        fetchPermissions(site.id),
        fetchClinicalServices(),
      ]);

    weekSummary = mapWeekSummary(ukWeekStart, ukWeekEnd, weekSummaryV2);
    permissions = permissionsV2;
    clinicalServices = clinicalServicesV2;
  } else {
    const [weekSummaryV1, permissionsV1, clinicalServicesV1] =
      await Promise.all([
        summariseWeek(ukWeekStart, ukWeekEnd, site.id),
        fetchPermissions(site.id),
        fetchClinicalServices(),
      ]);

    weekSummary = weekSummaryV1;
    permissions = permissionsV1;
    clinicalServices = clinicalServicesV1;
  }

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
