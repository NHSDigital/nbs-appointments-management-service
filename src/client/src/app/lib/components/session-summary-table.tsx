// import { Cell, Table } from '@components/nhsuk-frontend';
import { ClinicalService, SessionSummary } from '@types';
import Link from 'next/link';
import { UrlObject } from 'url';
import {
  dateTimeFormat,
  isAfterCalendarDateUk,
  parseToUkDatetime,
  ukNow,
} from '@services/timeService';
import { Cell } from './nhsuk-frontend';
import { SessionSummaryTableData } from './session-summary-table-data';

type SessionSummaryTableProps = {
  sessionSummaries: SessionSummary[];
  showChangeSessionLink?: {
    siteId: string;
    ukDate: string;
  };
  clinicalServices: ClinicalService[];
  showUnbooked?: boolean;
  showBooked?: boolean;
  tableCaption?: string;
};

export const SessionSummaryTable = ({
  sessionSummaries,
  showChangeSessionLink,
  clinicalServices,
  showUnbooked = true,
  showBooked = true,
  tableCaption,
}: SessionSummaryTableProps) => {
  const sessionSummaryRows = getSessionSummaryRows(
    sessionSummaries,
    clinicalServices,
    showUnbooked,
    showBooked,
    showChangeSessionLink,
  );

  return (
    <SessionSummaryTableData
      showChangeSessionLink={showChangeSessionLink}
      showUnbooked={showUnbooked}
      showBooked={showBooked}
      tableCaption={tableCaption}
      sessionSummaryRows={sessionSummaryRows}
    />
  );
};

export const getSessionSummaryRows = (
  sessionSummaries: SessionSummary[],
  clinicalServices: ClinicalService[],
  showUnbooked: boolean,
  showBooked: boolean,
  showChangeSessionLinkProps?: {
    siteId: string;
    ukDate: string;
  },
): Cell[][] =>
  sessionSummaries.map((sessionSummary, sessionIndex) => {
    const ukStartDatetime = parseToUkDatetime(
      sessionSummary.ukStartDatetime,
      dateTimeFormat,
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
      ...(showBooked
        ? [
            <SessionBookingsCell
              key={`session-${sessionIndex}-service-bookings`}
              sessionSummary={sessionSummary}
            />,
          ]
        : ['']),
      ...(showUnbooked
        ? [
            <SessionUnbookedCell
              key={`session-${sessionIndex}-unbooked`}
              sessionSummary={sessionSummary}
            />,
          ]
        : ['']),
      ...(showChangeSessionLinkProps &&
      isAfterCalendarDateUk(ukStartDatetime, ukNow())
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
  const ukStartDatetime = parseToUkDatetime(
    sessionSummary.ukStartDatetime,
    dateTimeFormat,
  );
  const ukEndDatetime = parseToUkDatetime(
    sessionSummary.ukEndDatetime,
    dateTimeFormat,
  );
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
      {Object.keys(sessionSummary.totalSupportedAppointmentsByService).map(
        (service, serviceIndex) => {
          return (
            <span key={`service-name-${serviceIndex}`}>
              {clinicalServices.find(cs => cs.value === service)?.label ??
                service}
              <br />
            </span>
          );
        },
      )}
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
      {Object.keys(sessionSummary.totalSupportedAppointmentsByService).map(
        (service, serviceIndex) => {
          return (
            <span key={`service-bookings-${serviceIndex}`}>
              {sessionSummary.totalSupportedAppointmentsByService[service]}{' '}
              booked
              <br />
            </span>
          );
        },
      )}
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
      {sessionSummary.maximumCapacity -
        sessionSummary.totalSupportedAppointments}{' '}
      unbooked
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
