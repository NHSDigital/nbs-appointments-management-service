import { Card, Table } from '@components/nhsuk-frontend';
import { getDetailedMonthView } from '@services/viewAvailabilityService';
import { Site } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  site: Site;
  searchMonth: dayjs.Dayjs;
};

export const WeekCardList = async ({ site, searchMonth }: Props) => {
  const weeks = await getDetailedMonthView(site, searchMonth);

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
            href={`view-availability/week?date=${week.startDate.format('YYYY-MM-DD')}`}
          >
            View week
          </Link>
        </Card>
      ))}
    </>
  );
};
