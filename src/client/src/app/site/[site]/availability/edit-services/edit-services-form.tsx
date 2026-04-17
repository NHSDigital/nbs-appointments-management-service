'use client';
import { SubmitHandler, useForm } from 'react-hook-form';
import {
  Site,
  SessionSummary,
  Session,
  AvailabilitySession,
  ClinicalService,
} from '@types';
import { useRouter } from 'next/navigation';
import { FormGroup, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import {
  dateTimeFormat,
  parseToUkDatetime,
  parseToTimeComponents,
  toTimeFormat,
} from '@services/timeService';
import { useTransition } from 'react';
import {
  SERVICE_TYPE_TITLES,
  groupServicesByType,
} from '@services/clinicalServices';
import { InsetText, Button, Checkboxes } from 'nhsuk-react-components';

export type RemoveServicesFormValues = {
  sessionToEdit: Session;
  newSession: Session;
  servicesToRemove: string[];
};

type Props = {
  date: string;
  site: Site;
  existingSession: SessionSummary;
  clinicalServices: ClinicalService[];
};

const EditServicesForm = ({
  site,
  existingSession,
  date,
  clinicalServices,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const existingUkStartTime = parseToUkDatetime(
    existingSession.ukStartDatetime,
    dateTimeFormat,
  ).format('HH:mm');
  const existingUkEndTime = parseToUkDatetime(
    existingSession.ukEndDatetime,
    dateTimeFormat,
  ).format('HH:mm');

  const {
    handleSubmit,
    register,
    formState: { errors },
  } = useForm<RemoveServicesFormValues>({
    defaultValues: {
      sessionToEdit: {
        startTime: parseToTimeComponents(existingUkStartTime),
        endTime: parseToTimeComponents(existingUkEndTime),
        services: Object.keys(
          existingSession.totalSupportedAppointmentsByService,
        ).map(service => service),
        slotLength: existingSession.slotLength,
        capacity: existingSession.capacity,
      },
      newSession: {
        startTime: parseToTimeComponents(existingUkStartTime),
        endTime: parseToTimeComponents(existingUkEndTime),
        services: Object.keys(
          existingSession.totalSupportedAppointmentsByService,
        ).map(service => service),
        slotLength: existingSession.slotLength,
        capacity: existingSession.capacity,
      },
      servicesToRemove: [],
    },
  });

  const router = useRouter();

  const submitForm: SubmitHandler<RemoveServicesFormValues> = async (
    form: RemoveServicesFormValues,
  ) => {
    startTransition(async () => {
      const encode = (obj: unknown) => btoa(JSON.stringify(obj));

      const sessionToEdit: AvailabilitySession = {
        from: toTimeFormat(form.sessionToEdit.startTime) ?? '',
        until: toTimeFormat(form.sessionToEdit.endTime) ?? '',
        slotLength: form.sessionToEdit.slotLength,
        capacity: form.sessionToEdit.capacity,
        services: form.sessionToEdit.services,
      };

      const servicesRemovedSession: AvailabilitySession = {
        from: toTimeFormat(form.newSession.startTime) ?? '',
        until: toTimeFormat(form.newSession.endTime) ?? '',
        slotLength: form.newSession.slotLength,
        capacity: form.newSession.capacity,
        services: form.servicesToRemove,
      };

      const reroute = `/site/${site.id}/availability/edit-services/confirmation?removedServicesSession=${encode(servicesRemovedSession)}&date=${date}&session=${encode(existingSession)}&sessionToEdit=${encode(sessionToEdit)}`;

      router.push(reroute);
    });
  };

  const clinicalServicesInSession = clinicalServices.filter(
    service =>
      Object.keys(
        existingSession.totalSupportedAppointmentsByService,
      ).findIndex(x => x === service.value) !== -1,
  );

  const groupedServices = groupServicesByType(clinicalServicesInSession);

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <InsetText>
        <p>If you need to remove all services, cancel the session instead.</p>
      </InsetText>
      <FormGroup error={errors.servicesToRemove?.message}>
        {Object.entries(groupedServices).map(([serviceType, services]) => {
          const groupTitle = SERVICE_TYPE_TITLES[serviceType] || serviceType;

          return (
            <fieldset
              key={serviceType}
              className="nhsuk-fieldset app-checkbox-group"
              style={{ marginBottom: '32px' }}
            >
              <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--s">
                {groupTitle}
              </legend>
              <Checkboxes>
                {services.map(clinicalService => {
                  return (
                    <Checkboxes.Item
                      id={`checkbox-${clinicalService.value}`}
                      value={clinicalService.value}
                      key={`checkbox-${clinicalService.value}`}
                      {...register('servicesToRemove', {
                        validate: value => {
                          if (value === undefined || value.length < 1) {
                            return 'Select a service to remove';
                          }
                          if (
                            value.length === clinicalServicesInSession.length
                          ) {
                            return 'Cancel this session if you need to remove all services';
                          }
                        },
                      })}
                    >
                      {clinicalService.label.replace('-', ' to ')}
                    </Checkboxes.Item>
                  );
                })}
              </Checkboxes>
            </fieldset>
          );
        })}
      </FormGroup>

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Continue</Button>
      )}
    </form>
  );
};

export default EditServicesForm;
