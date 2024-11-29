import { Card, Pagination, Table } from '@components/nhsuk-frontend';
import { Week } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  weeks: Week[];
  searchMonth: dayjs.Dayjs;
};

export const ViewAvailabilityPage = ({ weeks, searchMonth }: Props) => {
  const nextMonth = searchMonth.startOf('month').add(1, 'month');
  const previousMonth = searchMonth.startOf('month').subtract(1, 'month');

  const next = {
    title: nextMonth.format('MMMM YYYY'),
    href: `view-availability?date=${nextMonth.format('YYYY-MM-DD')}`,
  };
  const previous = {
    title: previousMonth.format('MMMM YYYY'),
    href: `view-availability?date=${previousMonth.format('YYYY-MM-DD')}`,
  };

  return (
    <>
      <Pagination previous={previous} next={next} />
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
            href={`view-availability/week?from=${week.startDate.format('YYYY-MM-DD')}&until=${week.endDate.format('YYYY-MM-DD')}`}
          >
            View week
          </Link>
        </Card>
      ))}
    </>
  );
};
