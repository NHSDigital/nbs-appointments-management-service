'use server';
import {
  ApplyAvailabilityTemplateRequest,
  SetAvailabilityRequest,
  Site,
} from '@types';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import {
  applyAvailabilityTemplate,
  saveAvailability,
} from '@services/appointmentsService';
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

  if (formData.sessionType === 'repeating') {
    const request: ApplyAvailabilityTemplateRequest = {
      site: site.id,
      from: startDate.format('YYYY-MM-DD'),
      until: endDate.format('YYYY-MM-DD'),
      template: {
        days: formData.days,
        sessions: [
          {
            from: formatTimeString(formData.session.startTime) ?? '',
            until: formatTimeString(formData.session.endTime) ?? '',
            slotLength: formData.session.slotLength,
            capacity: formData.session.capacity,
            services: formData.session.services,
          },
        ],
      },
      mode: 'Additive',
    };

    await applyAvailabilityTemplate(request);
  } else {
    const request: SetAvailabilityRequest = {
      site: site.id,
      date: startDate.format('YYYY-MM-DD'),
      sessions: [
        {
          from: formatTimeString(formData.session.startTime) ?? '',
          until: formatTimeString(formData.session.endTime) ?? '',
          slotLength: formData.session.slotLength,
          capacity: formData.session.capacity,
          services: formData.session.services,
        },
      ],
      mode: 'Additive',
    };

    await saveAvailability(request);
  }
}

export default saveAvailabilityTemplate;
