'use client';

import { Table } from 'nhsuk-react-components';

type DataProps = {
  rows: string[][];
};

export const AvailabilityCreatedEventsTableData = ({ rows }: DataProps) => {
  return (
    <Table>
      <Table.Head>
        <Table.Row>
          <Table.Cell>Dates</Table.Cell>
          <Table.Cell>Days</Table.Cell>
          <Table.Cell>Services</Table.Cell>
          <Table.Cell>Session type</Table.Cell>
        </Table.Row>
      </Table.Head>
      <Table.Body>
        {rows.map((row, index) => (
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
