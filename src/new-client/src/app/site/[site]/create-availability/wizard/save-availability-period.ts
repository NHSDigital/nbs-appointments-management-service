'use server';
import { SaveAvailabilityRequest, Site } from '@types';
import { AvailabilityPeriodFormValues } from './availability-period-wizard';
import { saveAvailability } from '@services/appointmentsService';
import { parseAndValidateDateFromComponents } from '@services/timeService';

async function saveAvailabilityPeriod(
  availabilityPeriod: AvailabilityPeriodFormValues,
  site: Site,
) {
  const startDate = parseAndValidateDateFromComponents(
    availabilityPeriod.startDateDay,
    availabilityPeriod.startDateMonth,
    availabilityPeriod.startDateYear,
  );
  const endDate = parseAndValidateDateFromComponents(
    availabilityPeriod.endDateDay,
    availabilityPeriod.endDateMonth,
    availabilityPeriod.endDateYear,
  );
  if (startDate === undefined || endDate === undefined) {
    throw new Error(
      'Could not parse dates - this should have been caught in form validation.',
    );
  }

  const request: SaveAvailabilityRequest = {
    date: startDate, // What should this be?
    site: site.id,
    sessions: [
      {
        from: startDate,
        until: endDate,
        // TODO: Get these values from form
        services: ['COVID:75'],
        slotLength: 5,
        capacity: 1,
      },
    ],
  };

  await saveAvailability(request);
}

export default saveAvailabilityPeriod;
