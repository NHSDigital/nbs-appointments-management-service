import { Cell, Table } from '@components/nhsuk-frontend';
import { clinicalServices, SessionSummary } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';
import { UrlObject } from 'url';

type SessionSummaryTableProps = {
  sessionSummaries: SessionSummary[];
  showChangeSessionLink?: {
    siteId: string;
    date: dayjs.Dayjs;
  };
};

export const SessionSummaryTable = ({
  sessionSummaries,
  showChangeSessionLink,
}: SessionSummaryTableProps) => {
  return (
    <Table
      headers={[
        'Time',
        'Services',
        'Booked',
        'Unbooked',
        ...(showChangeSessionLink ? ['Action'] : []),
      ]}
      rows={getSessionSummaryRows(sessionSummaries, showChangeSessionLink)}
    />
  );
};

export const getSessionSummaryRows = (
  sessionSummaries: SessionSummary[],
  showChangeSessionLinkProps?: {
    siteId: string;
    date: dayjs.Dayjs;
  },
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
      ...(showChangeSessionLinkProps
        ? [
            <Link
              key={`session-${sessionIndex}-action-link`}
              href={buildEditSessionLink(
                showChangeSessionLinkProps.siteId,
                showChangeSessionLinkProps.date,
                sessionSummary,
              )}
            >
              Change
            </Link>,
          ]
        : []),
    ];
  });

const buildEditSessionLink = (
  siteId: string,
  date: dayjs.Dayjs,
  sessionSummary: SessionSummary,
): UrlObject => {
  const encodedSummary = btoa(JSON.stringify(sessionSummary));

  const editSessionLink: UrlObject = {
    pathname: `/site/${siteId}/view-availability/week/edit-session`,
    query: {
      date: date.format('YYYY-MM-DD'),
      session: encodedSummary,
    },
  };

  return editSessionLink;
};
