import { Card, Table } from '@components/nhsuk-frontend';
import PipeDelimitedLinks, {
  ActionLink,
} from '@components/pipe-delimited-links';
import { SessionSummaryTable } from '@components/session-summary-table';
import { isInTheFuture } from '@services/timeService';
import { DaySummary } from '@types';
import Link from 'next/link';

type DaySummaryCardProps = {
  daySummary: DaySummary;
  siteId: string;
  canManageAvailability: boolean;
};

export const DaySummaryCard = ({
  daySummary: {
    date,
    sessions,
    maximumCapacity,
    bookedAppointments,
    cancelledAppointments,
    orphanedAppointments,
    remainingCapacity,
  },
  siteId,
  canManageAvailability,
}: DaySummaryCardProps) => {
  if (sessions.length === 0) {
    const actionLinks: ActionLink[] = [
      isInTheFuture(date.format('YYYY-MM-DD')) &&
        canManageAvailability && {
          text: 'Add availability to this day',
          href: `/site/${siteId}/create-availability/wizard?date=${date.format('YYYY-MM-DD')}`,
        },
      cancelledAppointments > 0 && {
        text: 'View cancelled appointments',
        href: `daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=1`,
      },
      orphanedAppointments > 0 && {
        text: 'View manual cancellations',
        href: `daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=2`,
      },
    ].filter(p => p !== false);

    return (
      <Card title={date.format('dddd D MMMM')}>
        <div>No availability</div>
        <br />
        <PipeDelimitedLinks actionLinks={actionLinks} />
      </Card>
    );
  }

  const actionLinks: ActionLink[] = [
    {
      text: 'View daily appointments',
      href: `daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1`,
    },
    cancelledAppointments > 0 && {
      text: 'View cancelled appointments',
      href: `daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=1`,
    },
    orphanedAppointments > 0 && {
      text: 'View manual cancellations',
      href: `daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=2`,
    },
  ].filter(p => p !== false);

  return (
    <Card title={date.format('dddd D MMMM')}>
      <SessionSummaryTable
        sessionSummaries={sessions}
        showChangeSessionLink={
          canManageAvailability
            ? {
                siteId,
                date,
              }
            : undefined
        }
      />
      <br />
      {isInTheFuture(date.format('YYYY-MM-DD')) && canManageAvailability && (
        <Link
          className="nhsuk-link"
          href={`/site/${siteId}/create-availability/wizard?date=${date.format('YYYY-MM-DD')}`}
        >
          Add Session
        </Link>
      )}
      <br />
      <OrphanedAppointmentsMessage
        orphanedAppointments={orphanedAppointments}
      />
      <CancelledAppointmentsMessage
        cancelledAppointments={cancelledAppointments}
      />
      <div className="appointments-summary">
        <span>
          <strong>Total appointments: {maximumCapacity}</strong>
        </span>
        <span>Booked: {bookedAppointments + orphanedAppointments}</span>
        <span>Unbooked: {remainingCapacity}</span>
      </div>
      <PipeDelimitedLinks actionLinks={actionLinks} />
    </Card>
  );
};

const OrphanedAppointmentsMessage = ({
  orphanedAppointments,
}: {
  orphanedAppointments: number;
}) => {
  if (orphanedAppointments === 0) {
    return null;
  }

  return orphanedAppointments === 1 ? (
    <div>
      There is <strong>1</strong> manual cancellation on this day.
    </div>
  ) : (
    <div>
      There are <strong>{orphanedAppointments}</strong> orphaned appointments on
      this day.
    </div>
  );
};

const CancelledAppointmentsMessage = ({
  cancelledAppointments,
}: {
  cancelledAppointments: number;
}) => {
  if (cancelledAppointments === 0) {
    return null;
  }

  return cancelledAppointments === 1 ? (
    <div>
      There is <strong>{cancelledAppointments}</strong> cancelled appointment on
      this day.
    </div>
  ) : (
    <div>
      There are <strong>{cancelledAppointments}</strong> cancelled appointments
      on this day.
    </div>
  );
};
