import { AppointmentCountsSummary } from '@components/appointment-counts-summary';

import PipeDelimitedLinks, {
  ActionLink,
} from '@components/pipe-delimited-links';
import { SessionSummaryTable } from '@components/session-summary-table';
import { RFC3339Format, isFutureCalendarDateUk } from '@services/timeService';
import { ClinicalService, DaySummary } from '@types';
import Link from 'next/link';
import { Card } from 'nhsuk-react-components';

type DaySummaryCardProps = {
  daySummary: DaySummary;
  siteId: string;
  canManageAvailability: boolean;
  clinicalServices: ClinicalService[];
  canViewDailyAppointments: boolean;
  cancelDayFlag: boolean;
};

export const DaySummaryCard = ({
  daySummary,
  siteId,
  canManageAvailability,
  clinicalServices,
  canViewDailyAppointments,
  cancelDayFlag,
}: DaySummaryCardProps) => {
  const {
    ukDate,
    sessions,
    bookedAppointments,
    orphanedAppointments,
    cancelledAppointments,
  } = daySummary;

  const isFutureCalendarDate = isFutureCalendarDateUk(ukDate);

  const totalAppointments = bookedAppointments + orphanedAppointments;

  if (sessions.length === 0) {
    const actionLinks: ActionLink[] = [
      isFutureCalendarDate &&
        canManageAvailability && {
          text: 'Add availability to this day',
          href: `/site/${siteId}/create-availability/wizard?date=${ukDate.format(RFC3339Format)}`,
        },
      //logically if sessions is empty, then this could just be replace by orphanedAppointments > 0...
      //as bookedAppointments (supported) SHOULD be zero?
      canViewDailyAppointments &&
        totalAppointments > 0 && {
          text: 'View daily appointments',
          href: `daily-appointments?date=${ukDate.format(RFC3339Format)}&page=1`,
        },
      canViewDailyAppointments &&
        cancelledAppointments > 0 && {
          text: 'View cancelled appointments',
          href: `daily-appointments?date=${ukDate.format(RFC3339Format)}&page=1&tab=1`,
        },
    ].filter(p => p !== false);

    return (
      <Card>
        <Card.Heading>{ukDate.format('dddd D MMMM')}</Card.Heading>
        <div>No availability</div>
        <AppointmentCountsSummary period={daySummary} />
        <PipeDelimitedLinks actionLinks={actionLinks} />
      </Card>
    );
  }

  const actionLinks: ActionLink[] = [
    canViewDailyAppointments &&
      //TODO is the totalApps > 0 logic check needed?? wasn't there before...
      //why was this link available if no scheduled appts existed??
      totalAppointments > 0 && {
        text: 'View daily appointments',
        href: `daily-appointments?date=${ukDate.format(RFC3339Format)}&page=1`,
      },
    canViewDailyAppointments &&
      cancelledAppointments > 0 && {
        text: 'View cancelled appointments',
        href: `daily-appointments?date=${ukDate.format(RFC3339Format)}&page=1&tab=1`,
      },
  ].filter(p => p !== false);

  return (
    <Card>
      <Card.Heading size="m">{ukDate.format('dddd D MMMM')}</Card.Heading>
      {cancelDayFlag && canManageAvailability && isFutureCalendarDate ? (
        <Card.Action
          href={`${process.env.CLIENT_BASE_PATH}/site/${siteId}/cancel-day?date=${ukDate.format(RFC3339Format)}`}
        >
          Cancel day
        </Card.Action>
      ) : null}
      <SessionSummaryTable
        sessionSummaries={sessions}
        clinicalServices={clinicalServices}
        showChangeSessionLink={
          canManageAvailability
            ? {
                siteId,
                ukDate: ukDate.format(RFC3339Format),
              }
            : undefined
        }
      />
      {/* TODO remove due to 10.x css?? */}
      <br />
      {isFutureCalendarDate && canManageAvailability && (
        <Link
          className="nhsuk-link"
          href={`/site/${siteId}/create-availability/wizard?date=${ukDate.format(RFC3339Format)}`}
        >
          Add Session
        </Link>
      )}
      <AppointmentCountsSummary period={daySummary} />
      <PipeDelimitedLinks actionLinks={actionLinks} />
    </Card>
  );
};
