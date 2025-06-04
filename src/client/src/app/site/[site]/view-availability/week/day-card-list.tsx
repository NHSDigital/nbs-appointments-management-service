import { summariseWeek } from '@services/availabilityCalculatorService';
import {
  ClinicalService,
  DaySummary,
  DaySummaryV2,
  SessionSummary,
  SessionSummaryV2,
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

const generateWeekSummary = (
  ukWeekStart: DayJsType,
  ukWeekEnd: DayJsType,
  weekSummaryV2: WeekSummaryV2,
): WeekSummary => {
  return {
    startDate: ukWeekStart,
    endDate: ukWeekEnd,
    daySummaries: generateDaySummaries(weekSummaryV2.daySummaries),
    maximumCapacity: weekSummaryV2.maximumCapacity,
    remainingCapacity: weekSummaryV2.remainingCapacity,
    bookedAppointments: weekSummaryV2.totalBooked,
    orphanedAppointments: weekSummaryV2.totalOrphaned,
    cancelledAppointments: weekSummaryV2.totalCancelled,
  };
};

const generateDaySummaries = (daySummaries: DaySummaryV2[]): DaySummary[] => {
  return daySummaries.map(daySummaryV2 => {
    return {
      ukDate: parseToUkDatetime(daySummaryV2.date),
      maximumCapacity: daySummaryV2.maximumCapacity,
      remainingCapacity: daySummaryV2.remainingCapacity,
      bookedAppointments: daySummaryV2.totalBooked,
      orphanedAppointments: daySummaryV2.totalOrphaned,
      cancelledAppointments: daySummaryV2.totalCancelled,
      sessions: generateSessionSummaries(daySummaryV2.sessionSummaries),
    };
  });
};

const generateSessionSummaries = (
  sessionSummaries: SessionSummaryV2[],
): SessionSummary[] => {
  return sessionSummaries.map(sessionSummaryV2 => {
    return {
      ukStartDatetime: sessionSummaryV2.from,
      ukEndDatetime: sessionSummaryV2.until,
      maximumCapacity: sessionSummaryV2.maximumCapacity,
      capacity: sessionSummaryV2.remainingCapacity,
      totalBookings: sessionSummaryV2.totalBooked,
      bookings: sessionSummaryV2.serviceBookings,
      //TODO do we need this??
      slotLength: 0,
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

    weekSummary = generateWeekSummary(ukWeekStart, ukWeekEnd, weekSummaryV2);
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
