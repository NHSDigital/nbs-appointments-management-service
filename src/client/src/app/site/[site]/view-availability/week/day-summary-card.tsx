import { Card, Table } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import { isInTheFuture } from '@services/timeService';
import { DaySummary } from '@types';
import Link from 'next/link';

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
    remainingCapacity,
  },
  siteId,
}: DaySummaryCardProps) => {
  const hasAvailability = sessions.length > 0;

  return (
    <Card title={date.format('dddd D MMMM')}>
      {hasAvailability ? (
        <SessionSummaryTable
          sessionSummaries={sessions}
          showChangeSessionLink={{
            siteId,
            date,
          }}
        />
      ) : (
        <div>No availability</div>
      )}
      <br />
      {isInTheFuture(date.format('YYYY-MM-DD')) && (
        <Link
          className="nhsuk-link"
          href={`/site/${siteId}/create-availability/wizard?date=${date.format('YYYY-MM-DD')}`}
        >
          Add Session
        </Link>
      )}
      {hasAvailability && (
        <Table
          headers={[
            `Total appointments: ${maximumCapacity}`,
            `Booked: ${bookedAppointments}`,
            `Unbooked: ${remainingCapacity}`,
          ]}
          rows={[]}
        />
      )}
      <br />
      <Link
        className="nhsuk-link"
        href={`daily-appointments?date=${date.format('YYYY-MM-DD')}&page=1`}
      >
        View daily appointments
      </Link>
    </Card>
  );
};
