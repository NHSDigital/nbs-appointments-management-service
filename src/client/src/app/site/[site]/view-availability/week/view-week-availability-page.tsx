import { Card, Table } from '@components/nhsuk-frontend';
import { DayAvailabilityDetails } from '@types';
import dayjs from 'dayjs';

type Props = {
  days: DayAvailabilityDetails[];
};

export const ViewWeekAvailabilityPage = ({ days }: Props) => {
  return (
    <>
      {days.map((d, i) => (
        <Card title={dayjs(d.date).format('dddd D MMMM')} key={i}>
          <Table
            headers={['Time', 'Services', 'Booked', 'Unbooked']}
            rows={d.serviceInformation.map(session => {
              return [
                `${session.time}`,
                session.serviceDetails.map((sd, k) => {
                  return (
                    <span key={k}>
                      {sd.service}
                      <br />
                    </span>
                  );
                }),
                session.serviceDetails.map((sd, j) => {
                  return (
                    <span key={j}>
                      {sd.booked} booked
                      <br />
                    </span>
                  );
                }),
                `${session.unbooked ?? 0} unbooked`,
              ];
            })}
          ></Table>
          <br />
          {/* TODO: Add link for add session */}
          <Table
            headers={[
              `Total appointments: ${d.totalAppointments}`,
              `Booked: ${d.booked}`,
              `Unbooked: ${d.unbooked}`,
            ]}
            rows={[]}
          ></Table>
          <br />
          {/* TODO: Add link to view daily appointments */}
        </Card>
      ))}
    </>
  );
};
