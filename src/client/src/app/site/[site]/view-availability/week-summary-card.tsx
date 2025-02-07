import { AppointmentCountsSummary } from '@components/appointment-counts-summary';
import { Card, Table } from '@components/nhsuk-frontend';
import { clinicalServices, WeekSummary } from '@types';
import Link from 'next/link';

type WeekSummaryCardProps = {
  weekSummary: WeekSummary;
};

export const WeekSummaryCard = ({ weekSummary }: WeekSummaryCardProps) => {
  const { startDate, endDate, daySummaries } = weekSummary;

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

      <AppointmentCountsSummary period={weekSummary} />
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
