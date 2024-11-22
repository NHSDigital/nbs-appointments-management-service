import { Card, Table } from '@components/nhsuk-frontend';
import { clinicalServices, Week } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type Props = {
  weeks: Week[];
};

export const ViewAvailabilityPage = ({ weeks }: Props) => {
  const totalAppts = 2800;
  const booked = 2355;

  return (
    <>
      {weeks.map((week, i) => (
        <Card
          title={`${week.start} ${dayjs().month(week.startMonth).format('MMMM')} to ${week.end} ${dayjs().month(week.endMonth).format('MMMM')}`}
          key={i}
        >
          <Table
            headers={['Services', 'Booked appointments']}
            rows={clinicalServices.map(service => {
              return [service.label, service.value];
            })}
          ></Table>
          <Table
            headers={[
              `Total appointments: ${totalAppts}`,
              `Booked: ${booked}`,
              `Unbooked: ${week.unbooked}`,
            ]}
            rows={[]}
          ></Table>
          <br />
          <Link href="">View week</Link>
        </Card>
      ))}
    </>
  );
};
