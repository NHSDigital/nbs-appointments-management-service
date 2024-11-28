import { Card, Table } from '@components/nhsuk-frontend';
import { Week } from '@types';
import Link from 'next/link';

type Props = {
  weeks: Week[];
};

export const ViewAvailabilityPage = ({ weeks }: Props) => {
  return (
    <>
      {weeks.map((week, i) => (
        <Card
          title={`${week.startDate.format('D MMMM')} to ${week.endDate.format('D MMMM')}`}
          key={i}
        >
          <Table
            headers={['Services', 'Booked appointments']}
            rows={week.bookedAppointments.map(appts => {
              return [appts.service, appts.count];
            })}
          ></Table>
          <Table
            headers={[
              `Total appointments: ${week.totalAppointments}`,
              `Booked: ${week.booked}`,
              `Unbooked: ${week.unbooked}`,
            ]}
            rows={[]}
          ></Table>
          <br />
          <Link
            className="nhsuk-link"
            href={`view-availability/week?from=${week.startDate.format('YYYY-MM-DD')}&to=${week.endDate.format('YYYY-MM-DD')}`}
          >
            View week
          </Link>
        </Card>
      ))}
    </>
  );
};
