'use client';
import { Table } from 'nhsuk-react-components';
import { Cell } from './nhsuk-frontend';

type DataProps = {
  showChangeSessionLink?: {
    siteId: string;
    ukDate: string;
  };
  showUnbooked?: boolean;
  showBooked?: boolean;
  tableCaption?: string;
  sessionSummaryRows: Cell[][];
};

export const SessionSummaryTableData = ({
  showChangeSessionLink,
  showUnbooked,
  showBooked,
  tableCaption,
  sessionSummaryRows,
}: DataProps) => {
  return (
    <Table caption={tableCaption}>
      <Table.Head>
        <Table.Row>
          <Table.Cell>Time</Table.Cell>
          <Table.Cell>Services</Table.Cell>
          {showBooked && <Table.Cell>Booked</Table.Cell>}
          {showUnbooked && <Table.Cell>Unbooked</Table.Cell>}
          {showChangeSessionLink && <Table.Cell>Action</Table.Cell>}
        </Table.Row>
      </Table.Head>
      <Table.Body>
        {sessionSummaryRows.map((row, index) => (
          <Table.Row key={index}>
            {row.map((cell, cellIndex) => (
              <Table.Cell key={cellIndex}>{cell}</Table.Cell>
            ))}
          </Table.Row>
        ))}
      </Table.Body>
    </Table>
  );
};
