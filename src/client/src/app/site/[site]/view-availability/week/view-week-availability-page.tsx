import { Card, Table } from '@components/nhsuk-frontend';
import { DailyAvailability } from '@types';
import dayjs from 'dayjs';

type Props = {
  availability: DailyAvailability[];
};

export const ViewWeekAvailabilityPage = ({ availability }: Props) => {
  return (
    <>
      {availability.map((day, i) => (
        <Card title={dayjs(day.date).format('dddd D MMMM')} key={i}>
          <Table
            headers={['Time', 'Services', 'Booked', 'Unbooked', 'Action']}
            rows={day.sessions.map(session => {
              return [
                `${session.from} - ${session.until}`,
                `${session.services.join(',')}`,
                '0 booked',
                '0 unbooked',
                'Change', // TODO: This should be a link
              ];
            })}
          ></Table>
          <br />
          {/* TODO: Add link for add session */}
          <Table
            headers={['Total appointments: 0', 'Booked: 0', 'Unbooked: 0']}
            rows={[]}
          ></Table>
          <br />
          {/* TODO: Add link to view daily appointments */}
        </Card>
      ))}
    </>
  );
};
