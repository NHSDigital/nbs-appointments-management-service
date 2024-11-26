import { Card, Table } from '@components/nhsuk-frontend';
import { Week } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  weeks: Week[];
};

export const ViewAvailabilityPage = ({ weeks }: Props) => {
  return (
    <>
      {weeks.map((week, i) => (
        <Card
          title={`${week.start} ${dayjs().month(week.startMonth).format('MMMM')} to ${week.end} ${dayjs().month(week.endMonth).format('MMMM')}`}
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
          <Link href="" className="nhsuk-link">
            View week
          </Link>
        </Card>
      ))}
    </>
  );
};
