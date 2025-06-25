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
import { Fragment } from 'react';

export type RemoveServicesFormValues = {
  sessionToEdit: Session;
  newSession: Session;
  servicesToRemove: ClinicalService[];
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
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<RemoveServicesFormValues>({
    defaultValues: {
      sessionToEdit: {
        startTime: parseToTimeComponents(existingUkStartTime),
        endTime: parseToTimeComponents(existingUkEndTime),
        services: Object.keys(existingSession.bookings).map(service => service),
        slotLength: existingSession.slotLength,
        capacity: existingSession.capacity,
      },
      newSession: {
        startTime: parseToTimeComponents(existingUkStartTime),
        endTime: parseToTimeComponents(existingUkEndTime),
        services: Object.keys(existingSession.bookings).map(service => service),
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
    const remainingServices = form.newSession.services.filter(
      service =>
        form.servicesToRemove.findIndex(x => x.value === service) === -1,
    );

    const updatedSession: AvailabilitySession = {
      from: toTimeFormat(form.newSession.startTime) ?? '',
      until: toTimeFormat(form.newSession.endTime) ?? '',
      slotLength: form.newSession.slotLength,
      capacity: form.newSession.capacity,
      services: remainingServices,
    };

    await editSession({
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
    });

    router.push(
      `edit/confirmed?updatedSession=${btoa(JSON.stringify(updatedSession))}&date=${date}`,
    );
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup error={errors.newSession?.services?.message}>
        <CheckBoxes>
          {clinicalServices.map(clinicalService => {
            if (
              Object.keys(existingSession.bookings)
                .map(service => service)
                .findIndex(x => x === clinicalService.value) === -1
            ) {
              // eslint-disable-next-line react/jsx-key, react/jsx-no-useless-fragment
              return <Fragment />;
            }
            return (
              <CheckBox
                id={`checkbox-${clinicalService.value}`}
                label={clinicalService.label}
                value={clinicalService.value}
                key={`checkbox-${clinicalService.value}`}
                {...register('servicesToRemove', {
                  validate: value => {
                    if (value === undefined || value.length < 1) {
                      return 'Select a service';
                    }
                  },
                })}
              />
            );
          })}
        </CheckBoxes>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Continue</Button>
      )}
    </form>
  );
};

export default EditServicesForm;
