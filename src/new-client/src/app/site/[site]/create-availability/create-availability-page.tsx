import { Button, Table } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { AvailabilityCreatedEvent, Site, clinicalServices } from '@types';
import { fetchAvailabilityCreatedEvents } from '@services/appointmentsService';
import { getDayOfWeek, parseDateString } from '@services/timeService';

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
        `${parseDateString(availability.from).format('D MMMM YYYY')} - ${parseDateString(availability.to ?? '').format('D MMMM YYYY')}`, //.format('D MMMM YYYY')
        availability.template.days.map(dayToShortName).join(', '),
        availability.template.sessions[0].services
          .map(serviceValueToLabel)
          .join(', '),
        'Weekly repeating',
      ];
    }
    return [
      parseDateString(availability.from).format('D MMMM YYYY'),
      getDayOfWeek(parseDateString(availability.from)),
      availability.sessions
        ? availability.sessions[0].services.map(serviceValueToLabel)
        : 'Error',
      'Single date',
    ];
  });

  return { headers, rows };
};

const dayToShortName = (day: string) => {
  switch (day) {
    case 'Monday':
      return 'Mon';
    case 'Tuesday':
      return 'Tue';
    case 'Wednesday':
      return 'Wed';
    case 'Thursday':
      return 'Thu';
    case 'Friday':
      return 'Fri';
    case 'Saturday':
      return 'Sat';
    case 'Sunday':
      return 'Sun';
    default:
      return day;
  }
};

const serviceValueToLabel = (serviceValue: string) => {
  const service = clinicalServices.find(s => s.value === serviceValue);
  return service?.label;
};
