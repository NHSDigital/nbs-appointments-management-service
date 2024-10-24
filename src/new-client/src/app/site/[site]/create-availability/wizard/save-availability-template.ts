'use server';
import { ApplyAvailabilityTemplateRequest, Site } from '@types';
import { AvailabilityTemplateFormValues } from './availability-template-wizard';
import { saveAvailability } from '@services/appointmentsService';
import { parseAndValidateDateFromComponents } from '@services/timeService';

async function saveAvailabilityTemplate(
  availabilityTemplate: AvailabilityTemplateFormValues,
  site: Site,
) {
  const startDate = parseAndValidateDateFromComponents(
    availabilityTemplate.startDateDay,
    availabilityTemplate.startDateMonth,
    availabilityTemplate.startDateYear,
  );
  const endDate = parseAndValidateDateFromComponents(
    availabilityTemplate.endDateDay,
    availabilityTemplate.endDateMonth,
    availabilityTemplate.endDateYear,
  );
  // TODO: slimline this numbers -> dayjs conversion to be less hacky and avoid checks like this
  if (startDate === undefined || endDate === undefined) {
    throw new Error(
      'Could not parse dates - this should have been caught in form validation.',
    );
  }

  const request: ApplyAvailabilityTemplateRequest = {
    site: site.id,
    from: startDate.format('YYYY-MM-DD'),
    until: endDate.format('YYYY-MM-DD'),
    // TODO: Get this from the form values itself
    template: {
      days: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'],
      sessions: [
        {
          from: '09:00',
          until: '17:00',
          slotLength: 1,
          capacity: 2,
          services: ['COVID:12_15'],
        },
      ],
    },
  };

  await saveAvailability(request);
}

export default saveAvailabilityTemplate;
