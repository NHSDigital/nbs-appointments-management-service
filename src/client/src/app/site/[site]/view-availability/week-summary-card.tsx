import { Card, Table } from '@components/nhsuk-frontend';
import { clinicalServices, WeekSummary } from '@types';
import Link from 'next/link';

type WeekSummaryCardProps = {
  weekSummary: WeekSummary;
};

export const WeekSummaryCard = ({
  weekSummary: {
    startDate,
    endDate,
    daySummaries,
    bookedAppointments,
    maximumCapacity,
    orphanedAppointments,
    remainingCapacity,
  },
}: WeekSummaryCardProps) => {
  const allBookingsInWeek = daySummaries.reduce(
    (acc, daySummary) => {
      daySummary.sessions.forEach(session => {
        Object.entries(session.bookings).forEach(([key, value]) => {
          if (!acc[key]) {
            acc[key] = 0;
          }
          acc[key] = acc[key] + value;
        });
      });
      return acc;
    },
    {} as Record<string, number>,
  );

  return (
    <Card
      title={`${startDate.format('D MMMM')} to ${endDate.format('D MMMM')}`}
    >
      {Object.entries(allBookingsInWeek).length > 0 ? (
        <Table
          headers={['Services', 'Booked appointments']}
          rows={Object.entries(allBookingsInWeek).map(([service, count]) => [
            clinicalServices.find(cs => cs.value === service)?.label,
            count,
          ])}
        />
      ) : (
        <div>No availability</div>
      )}

      <div className="appointments-summary">
        <span>
          <strong>Total appointments: {maximumCapacity}</strong>
        </span>
        <span>Booked: {bookedAppointments + orphanedAppointments}</span>
        <span>Unbooked: {remainingCapacity}</span>
      </div>
      <br />
      <Link
        className="nhsuk-link"
        href={`view-availability/week?date=${startDate.format('YYYY-MM-DD')}`}
      >
        View week
      </Link>
    </Card>
  );
};
