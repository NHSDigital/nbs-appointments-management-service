'use client';
import { ClinicalService } from '@types';
import { Table } from 'nhsuk-react-components';

type BookingsTableProps = {
  bookings: Record<string, number>;
  clinicalServices: ClinicalService[];
};

export const BookingsTable = ({
  bookings,
  clinicalServices,
}: BookingsTableProps) => {
  return (
    <Table className="card-item-margin">
      <Table.Head>
        <Table.Row>
          <Table.Cell>Services</Table.Cell>
          <Table.Cell>Booked appointments</Table.Cell>
        </Table.Row>
      </Table.Head>
      <Table.Body>
        {Object.entries(bookings).map(([service, count]) => (
          <Table.Row key={service}>
            <Table.Cell>
              {clinicalServices.find(cs => cs.value === service)?.label ||
                service}
            </Table.Cell>
            <Table.Cell>{count}</Table.Cell>
          </Table.Row>
        ))}
      </Table.Body>
    </Table>
  );
};
