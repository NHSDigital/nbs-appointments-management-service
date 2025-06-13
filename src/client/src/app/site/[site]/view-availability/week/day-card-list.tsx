import { summariseWeek } from '@services/availabilityCalculatorService';
import {
  ClinicalService,
  DaySummary,
  DaySummaryV2,
  Site,
  WeekSummary,
  WeekSummaryV2,
} from '@types';
import { DaySummaryCard } from './day-summary-card';
import {
  fetchClinicalServices,
  fetchFeatureFlag,
  fetchPermissions,
  fetchWeekSummaryV2,
} from '@services/appointmentsService';
import {
  dateFormat,
  DayJsType,
  emptyWeekSummary,
  parseToUkDatetime,
} from '@services/timeService';

type Props = {
  site: Site;
  ukWeekStart: DayJsType;
  ukWeekEnd: DayJsType;
};

const mapWeekSummary = (
  ukWeekStart: DayJsType,
  ukWeekEnd: DayJsType,
  weekSummaryV2: WeekSummaryV2,
): WeekSummary => {
  return {
    ...weekSummaryV2,
    startDate: ukWeekStart,
    endDate: ukWeekEnd,
    daySummaries: mapDaySummaries(weekSummaryV2.daySummaries),
  };
};

const mapDaySummaries = (daySummaries: DaySummaryV2[]): DaySummary[] => {
  return daySummaries.map(daySummaryV2 => {
    return {
      ...daySummaryV2,
      ukDate: parseToUkDatetime(daySummaryV2.date),
    };
  });
};

export const DayCardList = async ({ site, ukWeekStart, ukWeekEnd }: Props) => {
  let weekSummary: WeekSummary = emptyWeekSummary;
  let permissions: string[];
  let clinicalServices: ClinicalService[];

  const multipleServicesFlag = await fetchFeatureFlag('MultipleServices');

  if (multipleServicesFlag.enabled) {
    const [weekSummaryV2, permissionsV2, clinicalServicesV2] =
      await Promise.all([
        fetchWeekSummaryV2(site.id, ukWeekStart.format(dateFormat)),
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
