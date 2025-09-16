'use client';
import {
  SmallSpinnerWithText,
  Button,
  FormGroup,
  RadioGroup,
  Radio,
} from '@components/nhsuk-frontend';
import { editSession } from '@services/appointmentsService';
import {
  dateTimeFormat,
  getNearestAlignedTimes,
  parseDateAndTimeComponentsToUkDateTime,
  parseToTimeComponents,
  parseToUkDatetime,
  toTimeFormat,
} from '@services/timeService';
import { AvailabilitySession, SessionSummary, Site } from '@types';
import { notFound, useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';

type EditSessionStartTimeFormValues = {
  newStartTime: string;
};

type Props = {
  date: string;
  site: Site;
  existingSession: SessionSummary;
  updatedSession: AvailabilitySession;
};

const EditSessionStartTimeForm = ({
  date,
  site,
  existingSession,
  updatedSession,
}: Props) => {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<EditSessionStartTimeFormValues>();
  const router = useRouter();

  const requestedStartTime = parseToTimeComponents(updatedSession.from);
  if (requestedStartTime === undefined) {
    notFound();
  }

  const parsedStartTime = parseDateAndTimeComponentsToUkDateTime(
    date,
    requestedStartTime,
  );

  const nearestOptions = getNearestAlignedTimes(
    parsedStartTime,
    existingSession.slotLength,
  );

  const submitForm = async (form: EditSessionStartTimeFormValues) => {
    const parsedTime = parseToTimeComponents(form.newStartTime) ?? '';
    const session = { ...updatedSession, from: toTimeFormat(parsedTime) ?? '' };

    const existingUkStartTime = parseToUkDatetime(
      existingSession.ukStartDatetime,
      dateTimeFormat,
    ).format('HH:mm');
    const existingUkEndTime = parseToUkDatetime(
      existingSession.ukEndDatetime,
      dateTimeFormat,
    ).format('HH:mm');

    await editSession({
      date,
      site: site.id,
      mode: 'Edit',
      sessions: [session],
      sessionToEdit: {
        from: existingUkStartTime,
        until: existingUkEndTime,
        capacity: existingSession.capacity,
        services: Object.keys(
          existingSession.totalSupportedAppointmentsByService,
        ).map(service => service),
        slotLength: existingSession.slotLength,
      },
    });

    router.push(
      `confirmed?updatedSession=${btoa(JSON.stringify(session))}&date=${date}`,
    );
  };

  return (
    <>
      <p>
        There are booked appointments in this sessions which means the start
        time cannot be changed to: {parsedStartTime.format('HH:mma')}
      </p>
      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup
          legend="What time do you want the session to start?"
          error={errors.newStartTime?.message}
        >
          <RadioGroup>
            {nearestOptions.map((option, index) => {
              return (
                <Radio
                  label={`${option.format('HH:mma')}`}
                  id={`start-time-option-${index}`}
                  key={index}
                  value={`${option.format('HH:mm')}`}
                  {...register('newStartTime', {
                    required: { value: true, message: 'Select an option' },
                  })}
                />
              );
            })}
          </RadioGroup>
        </FormGroup>

        {isSubmitting || isSubmitSuccessful ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit">Change session</Button>
        )}
      </form>
    </>
  );
};

export default EditSessionStartTimeForm;
