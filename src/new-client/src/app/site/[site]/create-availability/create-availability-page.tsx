import { Button, Table } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { AvailabilityCreatedEvent, Site, clinicalServices } from '@types';
import { fetchAvailabilityCreatedEvents } from '@services/appointmentsService';
import { parseDateString } from '@services/timeService';

type Props = {
  site: Site;
};

export const CreateAvailabilityPage = async ({ site }: Props) => {
  const response = await fetchAvailabilityCreatedEvents(site.id);

  const tableData = mapTableData(response);

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
        `${parseDateString(availability.from).format('D MMM YYYY')} - ${parseDateString(availability.to ?? '').format('D MMM YYYY')}`, //.format('D MMMM YYYY')
        availability.template.days.length === 7
          ? 'All'
          : availability.template.days.map(d => d.substring(0, 3)).join(', '),
        availability.template.sessions[0].services
          .map(serviceValueToLabel)
          .join(', '),
        'Weekly repeating',
      ];
    }
    return [
      parseDateString(availability.from).format('D MMM YYYY'),
      parseDateString(availability.from).format('ddd'),
      availability.sessions
        ? availability.sessions[0].services.map(serviceValueToLabel)
        : 'Error',
      'Single date',
    ];
  });

  return { headers, rows };
};

const serviceValueToLabel = (serviceValue: string) => {
  const service = clinicalServices.find(s => s.value === serviceValue);
  return service?.label;
};
