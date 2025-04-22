import { Cell, Table } from '@components/nhsuk-frontend';
import { ClinicalService, SessionSummary } from '@types';
import Link from 'next/link';
import { UrlObject } from 'url';
import {
  extractUkSessionDatetime,
  isAfter,
  ukNow,
} from '@services/timeService';

type SessionSummaryTableProps = {
  sessionSummaries: SessionSummary[];
  showChangeSessionLink?: {
    siteId: string;
    ukDate: string;
  };
  clinicalServices: ClinicalService[];
};

export const SessionSummaryTable = ({
  sessionSummaries,
  showChangeSessionLink,
  clinicalServices,
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
      rows={getSessionSummaryRows(
        sessionSummaries,
        clinicalServices,
        showChangeSessionLink,
      )}
    />
  );
};

export const getSessionSummaryRows = (
  sessionSummaries: SessionSummary[],
  clinicalServices: ClinicalService[],
  showChangeSessionLinkProps?: {
    siteId: string;
    ukDate: string;
  },
): Cell[][] =>
  sessionSummaries.map((sessionSummary, sessionIndex) => {
    const ukStartDatetime = extractUkSessionDatetime(
      sessionSummary.ukStartDatetime,
    );
    return [
      <SessionTimesCell
        key={`session-${sessionIndex}-start-and-end-time`}
        sessionSummary={sessionSummary}
      />,
      <SessionServicesCell
        key={`session-${sessionIndex}-service-name`}
        sessionSummary={sessionSummary}
        clinicalServices={clinicalServices}
      />,
      <SessionBookingsCell
        key={`session-${sessionIndex}-service-bookings`}
        sessionSummary={sessionSummary}
      />,
      <SessionUnbookedCell
        key={`session-${sessionIndex}-unbooked`}
        sessionSummary={sessionSummary}
      />,
      ...(showChangeSessionLinkProps && isAfter(ukStartDatetime, ukNow())
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
  const ukStartDatetime = extractUkSessionDatetime(
    sessionSummary.ukStartDatetime,
  );
  const ukEndDatetime = extractUkSessionDatetime(sessionSummary.ukEndDatetime);
  return (
    <strong>{`${ukStartDatetime.format('HH:mm')} - ${ukEndDatetime.format('HH:mm')}`}</strong>
  );
};

export const SessionServicesCell = ({
  sessionSummary,
  clinicalServices,
}: {
  sessionSummary: SessionSummary;
  clinicalServices: ClinicalService[];
}) => {
  return (
    <>
      {Object.keys(sessionSummary.bookings).map((service, serviceIndex) => {
        return (
          <span key={`service-name-${serviceIndex}`}>
            {clinicalServices.find(cs => cs.value === service)?.label ??
              service}
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
