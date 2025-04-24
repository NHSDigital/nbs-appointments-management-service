import { AppointmentCountsSummary } from '@components/appointment-counts-summary';
import { Card } from '@components/nhsuk-frontend';
import PipeDelimitedLinks, {
  ActionLink,
} from '@components/pipe-delimited-links';
import { SessionSummaryTable } from '@components/session-summary-table';
import { dateFormat, isFutureCalendarDateUk } from '@services/timeService';
import { ClinicalService, DaySummary } from '@types';
import Link from 'next/link';

type DaySummaryCardProps = {
  daySummary: DaySummary;
  siteId: string;
  canManageAvailability: boolean;
  clinicalServices: ClinicalService[];
};

export const DaySummaryCard = ({
  daySummary,
  siteId,
  canManageAvailability,
  clinicalServices,
}: DaySummaryCardProps) => {
  const { ukDate, sessions, cancelledAppointments, orphanedAppointments } =
    daySummary;

  const isFutureCalendarDate = isFutureCalendarDateUk(ukDate);

  if (sessions.length === 0) {
    const actionLinks: ActionLink[] = [
      isFutureCalendarDate &&
        canManageAvailability && {
          text: 'Add availability to this day',
          href: `/site/${siteId}/create-availability/wizard?date=${ukDate.format(dateFormat)}`,
        },
      cancelledAppointments > 0 && {
        text: 'View cancelled appointments',
        href: `daily-appointments?date=${ukDate.format(dateFormat)}&page=1&tab=1`,
      },
      orphanedAppointments > 0 && {
        text: 'View manual cancellations',
        href: `daily-appointments?date=${ukDate.format(dateFormat)}&page=1&tab=2`,
      },
    ].filter(p => p !== false);

    return (
      <Card title={ukDate.format('dddd D MMMM')}>
        <div>No availability</div>
        <AppointmentCountsSummary period={daySummary} />
        <PipeDelimitedLinks actionLinks={actionLinks} />
      </Card>
    );
  }

  const actionLinks: ActionLink[] = [
    {
      text: 'View daily appointments',
      href: `daily-appointments?date=${ukDate.format(dateFormat)}&page=1`,
    },
    cancelledAppointments > 0 && {
      text: 'View cancelled appointments',
      href: `daily-appointments?date=${ukDate.format(dateFormat)}&page=1&tab=1`,
    },
    orphanedAppointments > 0 && {
      text: 'View manual cancellations',
      href: `daily-appointments?date=${ukDate.format(dateFormat)}&page=1&tab=2`,
    },
  ].filter(p => p !== false);

  return (
    <Card title={ukDate.format('dddd D MMMM')}>
      <SessionSummaryTable
        sessionSummaries={sessions}
        clinicalServices={clinicalServices}
        showChangeSessionLink={
          canManageAvailability
            ? {
                siteId,
                ukDate: ukDate.format(dateFormat),
              }
            : undefined
        }
      />
      <br />
      {isFutureCalendarDate && canManageAvailability && (
        <Link
          className="nhsuk-link"
          href={`/site/${siteId}/create-availability/wizard?date=${ukDate.format(dateFormat)}`}
        >
          Add Session
        </Link>
      )}
      <AppointmentCountsSummary period={daySummary} />
      <PipeDelimitedLinks actionLinks={actionLinks} />
    </Card>
  );
};
