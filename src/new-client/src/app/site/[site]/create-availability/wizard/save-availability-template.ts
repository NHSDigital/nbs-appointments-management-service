'use server';
import { ApplyAvailabilityTemplateRequest, Site } from '@types';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { saveAvailability } from '@services/appointmentsService';
import { formatTimeString, parseDateComponents } from '@services/timeService';

async function saveAvailabilityTemplate(
  formData: CreateAvailabilityFormValues,
  site: Site,
) {
  const startDate = parseDateComponents(formData.startDate);
  const endDate = parseDateComponents(formData.endDate ?? formData.startDate);

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
    template: {
      days: formData.days,
      sessions: [
        {
          from: formatTimeString(formData.session.startTime),
          until: formatTimeString(formData.session.endTime),
          slotLength: formData.session.slotLength,
          capacity: formData.session.capacity,
          services: formData.session.services,
        },
      ],
    },
  };

  await saveAvailability(request);
}

export default saveAvailabilityTemplate;
