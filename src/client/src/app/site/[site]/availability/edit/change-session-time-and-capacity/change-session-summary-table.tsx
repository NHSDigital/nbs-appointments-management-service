import { Cell, Table } from '@components/nhsuk-frontend';
import { AvailabilitySession } from '@types';

type ChangeSessionSummaryTableProps = {
  sessionSummary: AvailabilitySession;
  tableCaption?: string;
};

export const ChangeSessionSummaryTable = ({
  sessionSummary,
  tableCaption,
}: ChangeSessionSummaryTableProps) => {
  return (
    <Table
      headers={['Time', 'Services']}
      rows={getSessionSummaryRows(sessionSummary)}
      caption={tableCaption}
    />
  );
};

const getSessionSummaryRows = (
  sessionSummary: AvailabilitySession,
): Cell[][] => {
  return [
    [
      <SessionTimesCell
        key={`session-start-and-end-time`}
        sessionSummary={sessionSummary}
      />,
      <SessionServicesCell
        key={`session-service-name`}
        sessionSummary={sessionSummary}
      />,
    ],
  ];
};

export const SessionTimesCell = ({
  sessionSummary,
}: {
  sessionSummary: AvailabilitySession;
}) => {
  return <strong>{`${sessionSummary.from} - ${sessionSummary.until}`}</strong>;
};

const SessionServicesCell = ({
  sessionSummary,
}: {
  sessionSummary: AvailabilitySession;
}) => {
  return (
    <>
      {sessionSummary.services.map((service, serviceIndex) => {
        return (
          <span key={`service-name-${serviceIndex}`}>
            {service}
            <br />
          </span>
        );
      })}
    </>
  );
};
