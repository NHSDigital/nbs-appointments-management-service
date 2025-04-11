import { Cell, Table } from '@components/nhsuk-frontend';
import { clinicalServices, SessionSummary } from '@types';
import Link from 'next/link';
import { UrlObject } from 'url';
import { isoTimezoneToDayjs, ukNow } from '@services/timeService';

type SessionSummaryTableProps = {
  sessionSummaries: SessionSummary[];
  showChangeSessionLink?: {
    siteId: string;
    ukDate: string;
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
    ukDate: string;
  },
): Cell[][] =>
  sessionSummaries.map((sessionSummary, sessionIndex) => {
    const ukStart = isoTimezoneToDayjs(sessionSummary.ukStart);
    return [
      <SessionTimesCell
        key={`session-${sessionIndex}-start-and-end-time`}
        sessionSummary={sessionSummary}
      />,
      <SessionServicesCell
        key={`session-${sessionIndex}-service-name`}
        sessionSummary={sessionSummary}
      />,
      <SessionBookingsCell
        key={`session-${sessionIndex}-service-bookings`}
        sessionSummary={sessionSummary}
      />,
      <SessionUnbookedCell
        key={`session-${sessionIndex}-unbooked`}
        sessionSummary={sessionSummary}
      />,
      ...(showChangeSessionLinkProps && ukStart.isAfter(ukNow())
        ? [
            <Link
              key={`session-${sessionIndex}-action-link`}
              href={buildEditSessionLink(
                showChangeSessionLinkProps.siteId,
                showChangeSessionLinkProps.ukDate,
                sessionSummary,
              )}
            >
              Change
            </Link>,
          ]
        : ['']),
    ];
  });

export const SessionTimesCell = ({
  sessionSummary,
}: {
  sessionSummary: SessionSummary;
}) => {
  const ukStart = isoTimezoneToDayjs(sessionSummary.ukStart);
  const ukEnd = isoTimezoneToDayjs(sessionSummary.ukEnd);
  return (
    <strong>{`${ukStart.format('HH:mm')} - ${ukEnd.format('HH:mm')}`}</strong>
  );
};

export const SessionServicesCell = ({
  sessionSummary,
}: {
  sessionSummary: SessionSummary;
}) => {
  return (
    <>
      {Object.keys(sessionSummary.bookings).map((service, serviceIndex) => {
        return (
          <span key={`service-name-${serviceIndex}`}>
            {clinicalServices.find(cs => cs.value === service)?.label}
            <br />
          </span>
        );
      })}
    </>
  );
};

export const SessionBookingsCell = ({
  sessionSummary,
}: {
  sessionSummary: SessionSummary;
}) => {
  return (
    <>
      {Object.keys(sessionSummary.bookings).map((service, serviceIndex) => {
        return (
          <span key={`service-bookings-${serviceIndex}`}>
            {sessionSummary.bookings[service]} booked
            <br />
          </span>
        );
      })}
    </>
  );
};

export const SessionUnbookedCell = ({
  sessionSummary,
}: {
  sessionSummary: SessionSummary;
}) => {
  return (
    <>
      {sessionSummary.maximumCapacity - sessionSummary.totalBookings} unbooked
    </>
  );
};

const buildEditSessionLink = (
  siteId: string,
  ukDate: string,
  sessionSummary: SessionSummary,
): UrlObject => {
  const encodedSummary = btoa(JSON.stringify(sessionSummary));

  const editSessionLink: UrlObject = {
    pathname: `/site/${siteId}/view-availability/week/edit-session`,
    query: {
      date: ukDate,
      session: encodedSummary,
    },
  };

  return editSessionLink;
};
