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
import { editSession } from '@services/appointmentsService';
import {
  Button,
  CheckBox,
  CheckBoxes,
  FormGroup,
  InsetText,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import {
  dateTimeFormat,
  parseToUkDatetime,
  parseToTimeComponents,
  toTimeFormat,
} from '@services/timeService';
import { useTransition } from 'react';
import fromServer from '@server/fromServer';
import {
  SERVICE_TYPE_TITLES,
  groupServicesByType,
} from '@services/clinicalServices';

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
  changeSessionUpliftedJourneyEnabled: boolean;
};

const EditServicesForm = ({
  site,
  existingSession,
  date,
  clinicalServices,
  changeSessionUpliftedJourneyEnabled,
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
      const remainingServices = form.newSession.services.filter(
        service => form.servicesToRemove.findIndex(x => x === service) === -1,
      );

      const encode = (obj: unknown) => btoa(JSON.stringify(obj));

      const updatedSession: AvailabilitySession = {
        from: toTimeFormat(form.newSession.startTime) ?? '',
        until: toTimeFormat(form.newSession.endTime) ?? '',
        slotLength: form.newSession.slotLength,
        capacity: form.newSession.capacity,
        services: remainingServices,
      };

      if (!changeSessionUpliftedJourneyEnabled) {
        await fromServer(
          editSession({
            date,
            site: site.id,
            mode: 'Edit',
            sessions: [updatedSession],
            sessionToEdit: {
              from: toTimeFormat(form.sessionToEdit.startTime) ?? '',
              until: toTimeFormat(form.sessionToEdit.endTime) ?? '',
              slotLength: form.sessionToEdit.slotLength,
              capacity: form.sessionToEdit.capacity,
              services: form.sessionToEdit.services,
            },
          }),
        );
      }

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

      let reroute = `/site/${site.id}/availability/`;
      if (changeSessionUpliftedJourneyEnabled) {
        reroute += `edit-services/confirmation?removedServicesSession=${encode(servicesRemovedSession)}&date=${date}&session=${encode(existingSession)}&sessionToEdit=${encode(sessionToEdit)}`;
      } else {
        reroute += `edit-services/confirmed?removedServicesSession=${encode(servicesRemovedSession)}&date=${date}`;
      }

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
              <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
                {groupTitle}
              </legend>
              <CheckBoxes>
                {services.map(clinicalService => {
                  return (
                    <CheckBox
                      id={`checkbox-${clinicalService.value}`}
                      label={clinicalService.label.replace('-', ' to ')}
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
                    />
                  );
                })}
              </CheckBoxes>
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
