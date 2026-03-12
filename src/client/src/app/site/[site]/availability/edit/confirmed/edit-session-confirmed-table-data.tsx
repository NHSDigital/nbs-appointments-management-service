'use client';
import { AvailabilitySession, ClinicalService } from '@types';
import { Table } from 'nhsuk-react-components';

type Props = {
  updatedSession: AvailabilitySession;
  clinicalServices: ClinicalService[];
};

export const EditSessionConfirmedTableData = ({
  updatedSession,
  clinicalServices,
}: Props) => {
  return (
    <Table>
      <Table.Head>
        <Table.Row>
          <Table.Cell>Time</Table.Cell>
          <Table.Cell>Services</Table.Cell>
        </Table.Row>
      </Table.Head>
      <Table.Body>
        <Table.Row>
          <Table.Cell>
            <strong key={`session-0-start-and-end-time`}>
              {`${updatedSession.from} - ${updatedSession.until}`}
            </strong>
          </Table.Cell>
          <Table.Cell>
            <>
              {updatedSession.services.map((service, serviceIndex) => {
                return (
                  <span key={`service-name-${serviceIndex}`}>
                    {clinicalServices.find(c => c.value === service)?.label ??
                      service}
                    <br />
                  </span>
                );
              })}
            </>
          </Table.Cell>
        </Table.Row>
      </Table.Body>
    </Table>
  );
};
