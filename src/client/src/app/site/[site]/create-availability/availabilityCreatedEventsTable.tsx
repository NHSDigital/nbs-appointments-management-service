import { Table } from '@components/nhsuk-frontend';
import { parseDateString } from '@services/timeService';
import { AvailabilityCreatedEvent, ClinicalService } from '@types';
import {
  fetchAvailabilityCreatedEvents,
  fetchClinicalServices,
} from '@services/appointmentsService';

type AvailabilityCreatedEventsTableProps = {
  siteId: string;
};

export const AvailabilityCreatedEventsTable = async ({
  siteId,
}: AvailabilityCreatedEventsTableProps) => {
  const availabilityCreatedEvents =
    await fetchAvailabilityCreatedEvents(siteId);
  const clinicalServices = await fetchClinicalServices();
  if (availabilityCreatedEvents.length === 0 || clinicalServices.length === 0) {
    return null;
  }

  return (
    <Table {...mapTableData(availabilityCreatedEvents, clinicalServices)} />
  );
};

const mapTableData = (
  availabilityCreated: AvailabilityCreatedEvent[],
  clinicalServices: ClinicalService[],
) => {
  const headers = ['Dates', 'Days', 'Services', 'Session type'];

  const rows = availabilityCreated.map(availability => {
    if (availability.template) {
      return [
        `${parseDateString(availability.from).format('D MMM YYYY')} - ${parseDateString(availability.to ?? '').format('D MMM YYYY')}`,
        availability.template.days.length === 7
          ? 'All'
          : availability.template.days.map(d => d.substring(0, 3)).join(', '),
        availability.template.sessions[0].services
          .map(service => serviceValueToLabel(service, clinicalServices))
          .join(', '),
        'Weekly repeating',
      ];
    }
    return [
      parseDateString(availability.from).format('D MMM YYYY'),
      parseDateString(availability.from).format('ddd'),
      availability.sessions
        ? availability.sessions[0].services.map(service =>
            serviceValueToLabel(service, clinicalServices),
          )
        : 'Error',
      'Single date',
    ];
  });

  return { headers, rows };
};

const serviceValueToLabel = (
  serviceValue: string,
  clinicalServices: ClinicalService[],
) => {
  const service = clinicalServices.find(s => s.value === serviceValue);
  return service?.label ?? serviceValue;
};
