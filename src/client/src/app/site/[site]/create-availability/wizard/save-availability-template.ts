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
import {
  dateFormat,
  toTimeFormat,
  parseDateComponentsToUkDatetime,
} from '@services/timeService';

async function saveAvailabilityTemplate(
  formData: CreateAvailabilityFormValues,
  site: Site,
) {
  const startUkDatetime = parseDateComponentsToUkDatetime(formData.startDate);
  const endUkDatetime = parseDateComponentsToUkDatetime(
    formData.endDate ?? formData.startDate,
  );

  // TODO: slimline this numbers -> dayjs conversion to be less hacky and avoid checks like this
  if (startUkDatetime === undefined || endUkDatetime === undefined) {
    throw new Error(
      'Could not parse dates - this should have been caught in form validation.',
    );
  }

  if (formData.sessionType === 'repeating') {
    const request: ApplyAvailabilityTemplateRequest = {
      site: site.id,
      from: startUkDatetime.format(dateFormat),
      until: endUkDatetime.format(dateFormat),
      template: {
        days: formData.days,
        sessions: [
          {
            from: toTimeFormat(formData.session.startTime) ?? '',
            until: toTimeFormat(formData.session.endTime) ?? '',
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
      date: startUkDatetime.format(dateFormat),
      sessions: [
        {
          from: toTimeFormat(formData.session.startTime) ?? '',
          until: toTimeFormat(formData.session.endTime) ?? '',
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
