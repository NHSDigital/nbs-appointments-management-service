import { Table } from '@components/nhsuk-frontend';
import { clinicalServices, SessionSummary } from '@types';
import dayjs from 'dayjs';

type SessionSummaryTableProps = {
  sessionSummary: SessionSummary;
};

export const SessionSummaryTable = ({
  sessionSummary,
}: SessionSummaryTableProps) => {
  return (
    <Table
      headers={['Time', 'Services', 'Booked', 'Unbooked']}
      rows={[
        [
          <strong
            key={0}
          >{`${dayjs(sessionSummary.start).format('HH:mm')} - ${dayjs(sessionSummary.end).format('HH:mm')}`}</strong>,
          Object.keys(sessionSummary.bookings).map((service, k) => {
            return (
              <span key={k}>
                {clinicalServices.find(cs => cs.value === service)?.label}
                <br />
              </span>
            );
          }),
          Object.keys(sessionSummary.bookings).map((service, j) => {
            return (
              <span key={j}>
                {sessionSummary.bookings[service]} booked
                <br />
              </span>
            );
          }),
          `${sessionSummary.maximumCapacity - sessionSummary.totalBookings} unbooked`,
        ],
      ]}
    />
  );
};
