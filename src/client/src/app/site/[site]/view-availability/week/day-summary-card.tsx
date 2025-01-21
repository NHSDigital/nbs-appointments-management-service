import { Card, Table } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { isInTheFuture } from '@services/timeService';
import { DaySummary } from '@types';
import Link from 'next/link';
import { ReactNode } from 'react';

type DaySummaryCardProps = {
  daySummary: DaySummary;
  siteId: string;
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
}: DaySummaryCardProps) => {
  if (sessions.length > 0) {
    return (
      <Card title={date.format('dddd D MMMM')}>
        <SessionSummaryTable
          sessionSummaries={sessions}
          showChangeSessionLink={{
            siteId,
            date,
          }}
        />
        <br />
        {isInTheFuture(date.format('YYYY-MM-DD')) && (
          <Link
            className="nhsuk-link"
            href={`/site/${siteId}/create-availability/wizard?date=${date.format('YYYY-MM-DD')}`}
          >
            Add Session
          </Link>
        )}
        <Table
          headers={[
            `Total appointments: ${maximumCapacity}`,
            `Booked: ${bookedAppointments}`,
            `Unbooked: ${remainingCapacity}`,
          ]}
          rows={[]}
        />
        <br />
        <Link
          className="nhsuk-link"
          href={`daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1`}
        >
          View daily appointments
        </Link>
      </Card>
    );
  }

  const actionLinks: ReactNode[] = [];
  if (cancelledAppointments > 0) {
    actionLinks.push(
      <Link
        className="nhsuk-link"
        href={`daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=1`}
      >
        View cancelled appointments
      </Link>,
    );
  }
  if (orphanedAppointments > 0) {
    actionLinks.push(
      <Link
        className="nhsuk-link"
        href={`daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1&tab=2`}
      >
        View manual cancellations
      </Link>,
    );
  }
  if (isInTheFuture(date.format('YYYY-MM-DD'))) {
    actionLinks.push(
      <Link
        className="nhsuk-link"
        href={`/site/${siteId}/create-availability/wizard?date=${date.format('YYYY-MM-DD')}`}
      >
        Add availability to this day
      </Link>,
    );
  }

  return (
    <Card title={date.format('dddd D MMMM')}>
      <div>No availability</div>
      <br />
      {actionLinks.reduce(
        (prev, curr, index) => (
          <>
            {prev}
            {index > 0 && ' | '}
            {curr}
          </>
        ),
        null,
      )}
    </Card>
  );
};
