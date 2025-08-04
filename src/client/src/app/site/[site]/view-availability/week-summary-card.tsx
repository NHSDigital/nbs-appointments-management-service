import { AppointmentCountsSummary } from '@components/appointment-counts-summary';
import { Card, Table } from '@components/nhsuk-frontend';
import { ClinicalService, WeekSummary } from '@types';
import Link from 'next/link';
import { RFC3339Format } from '@services/timeService';

type WeekSummaryCardProps = {
  ukWeekSummary: WeekSummary;
  clinicalServices: ClinicalService[];
};

export const WeekSummaryCard = ({
  ukWeekSummary,
  clinicalServices,
}: WeekSummaryCardProps) => {
  const { startDate, endDate, daySummaries } = ukWeekSummary;

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

      <AppointmentCountsSummary period={ukWeekSummary} />
      <br />
      <Link
        className="nhsuk-link"
        href={`view-availability/week?date=${startDate.format(RFC3339Format)}`}
      >
        View week
      </Link>
    </Card>
  );
};
