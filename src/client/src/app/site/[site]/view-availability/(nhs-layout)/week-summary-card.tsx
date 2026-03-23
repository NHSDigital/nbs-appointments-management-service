import { AppointmentCountsSummary } from '@components/appointment-counts-summary';
import { ClinicalService, WeekSummary } from '@types';
import { Card } from 'nhsuk-react-components';
import { BookingsTable } from './bookings-table';
import PipeDelimitedLinks from '@components/pipe-delimited-links';
import { isThisWeek, RFC3339Format } from '@services/timeService';

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

  const weekHeading = isThisWeek(startDate, endDate)
    ? `${startDate.format('D MMMM')} to ${endDate.format('D MMMM')} (this week)`
    : `${startDate.format('D MMMM')} to ${endDate.format('D MMMM')}`;

  return (
    <Card>
      <Card.Heading
        headingLevel="h3"
        className="appointment-summary-card-item-margin"
      >
        {weekHeading}
      </Card.Heading>
      {Object.entries(allBookingsInWeek).length > 0 ? (
        <BookingsTable
          bookings={allBookingsInWeek}
          clinicalServices={clinicalServices}
        />
      ) : (
        <div className="appointment-summary-card-item-margin">
          No availability
        </div>
      )}

      <AppointmentCountsSummary period={ukWeekSummary} />
      <PipeDelimitedLinks
        actionLinks={[
          {
            text: 'View week',
            href: `view-availability/week?date=${startDate.format(RFC3339Format)}`,
          },
        ]}
      />
    </Card>
  );
};
