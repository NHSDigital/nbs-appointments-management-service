import { Cell, Table } from '@components/nhsuk-frontend';
import { clinicalServices, SessionSummary } from '@types';
import dayjs from 'dayjs';

type SessionSummaryTableProps = {
  sessionSummaries: SessionSummary[];
};

export const SessionSummaryTable = ({
  sessionSummaries,
}: SessionSummaryTableProps) => {
  return (
    <Table
      headers={['Time', 'Services', 'Booked', 'Unbooked']}
      rows={getSessionSummaryRows(sessionSummaries)}
    />
  );
};

export const getSessionSummaryRows = (
  sessionSummaries: SessionSummary[],
): Cell[][] =>
  sessionSummaries.map((sessionSummary, sessionIndex) => {
    return [
      <strong
        key={`session-${sessionIndex}-start-and-end-time`}
      >{`${dayjs(sessionSummary.start).format('HH:mm')} - ${dayjs(sessionSummary.end).format('HH:mm')}`}</strong>,
      Object.keys(sessionSummary.bookings).map((service, serviceIndex) => {
        return (
          <span key={`session-${sessionIndex}-service-name-${serviceIndex}`}>
            {clinicalServices.find(cs => cs.value === service)?.label}
            <br />
          </span>
        );
      }),
      Object.keys(sessionSummary.bookings).map((service, serviceIndex) => {
        return (
          <span
            key={`session-${sessionIndex}-service-bookings-${serviceIndex}`}
          >
            {sessionSummary.bookings[service]} booked
            <br />
          </span>
        );
      }),
      `${sessionSummary.maximumCapacity - sessionSummary.totalBookings} unbooked`,
    ];
  });
