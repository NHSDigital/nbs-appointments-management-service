import { Button, Table } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { AvailabilityCreatedEvent, Site } from '@types';
import { fetchAvailabilityCreatedEvents } from '@services/appointmentsService';

type Props = {
  site: Site;
};

export const CreateAvailabilityPage = async ({ site }: Props) => {
  const availabilityCreated = await fetchAvailabilityCreatedEvents(site.id);
  const tableData = mapTableData(availabilityCreated);

  return (
    <>
      <p>
        You can create availability with multiple days and repeating sessions,
        to accurately reflect your site's capacity.
      </p>
      <br />
      {tableData && <Table {...tableData}></Table>}
      <br />
      <Link href={`/site/${site.id}/create-availability/wizard`}>
        <Button type="button">Create availablity</Button>
      </Link>
    </>
  );
};

const mapTableData = (availabilityCreated: AvailabilityCreatedEvent[]) => {
  if (!availabilityCreated.length) {
    return undefined;
  }

  const headers = ['Dates', 'Days', 'Services', 'Session type'];

  const rows = availabilityCreated.map(availability => {
    if (availability.template) {
      return [
        `${availability.from} - ${availability.to}`,
        availability.by,
        availability.template.days.join(', '),
        availability.template.sessions[0].services,
        'Repeat',
      ];
    }
    return [
      availability.from,
      availability.by,
      'N/A',
      availability.sessions ? availability.sessions[0].services : 'Error',
      'Single',
    ];
  });

  return { headers, rows };
};
