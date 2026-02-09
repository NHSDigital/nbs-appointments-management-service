import { AppointmentCountsSummary } from '@components/appointment-counts-summary';
import { ClinicalService, WeekSummary } from '@types';
import Link from 'next/link';
import { RFC3339Format } from '@services/timeService';
import { Card } from 'nhsuk-react-components';
import { BookingsTable } from './bookings-table';

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
        Object.entries(session.totalSupportedAppointmentsByService).forEach(
          ([key, value]) => {
            if (!acc[key]) {
              acc[key] = 0;
            }
            acc[key] = acc[key] + value;
          },
        );
      });
      return acc;
    },
    {} as Record<string, number>,
  );

  return (
    <Card>
      <Card.Heading>
        {startDate.format('D MMMM')} to {endDate.format('D MMMM')}
      </Card.Heading>
      {Object.entries(allBookingsInWeek).length > 0 ? (
        <BookingsTable
          bookings={allBookingsInWeek}
          clinicalServices={clinicalServices}
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
