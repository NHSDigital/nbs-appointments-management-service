'use client';
import {
  SmallSpinnerWithText,
  Button,
  FormGroup,
  RadioGroup,
  Radio,
} from '@components/nhsuk-frontend';
import {
  getNearestAlignedTimes,
  parseDateAndTimeComponentsToUkDateTime,
  parseToTimeComponents,
  toTimeFormat,
} from '@services/timeService';
import { AvailabilitySession, Session, SessionSummary, Site } from '@types';
import { notFound, useRouter } from 'next/navigation';
import { useTransition } from 'react';
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
  const [pendingSubmit, startTransition] = useTransition();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EditSessionStartTimeFormValues>();
  const router = useRouter();

  const originalStartTimeFormat =
    toTimeFormat(existingSession.ukStartDatetime) ?? '';
  const originalStart = parseToTimeComponents(originalStartTimeFormat);
  const requestedStartTime = parseToTimeComponents(updatedSession.from);
  if (requestedStartTime === undefined || originalStart === undefined) {
    notFound();
  }

  const parsedStartTime = parseDateAndTimeComponentsToUkDateTime(
    date,
    requestedStartTime,
  );
  const originalStartTime = parseDateAndTimeComponentsToUkDateTime(
    date,
    originalStart,
  );

  const nearestOptions = getNearestAlignedTimes(
    parsedStartTime,
    existingSession.slotLength,
    originalStartTime,
  );

  const submitForm = async (form: EditSessionStartTimeFormValues) => {
    startTransition(async () => {
      const parsedTime = parseToTimeComponents(form.newStartTime) ?? '';
      const session = {
        ...updatedSession,
        from: toTimeFormat(parsedTime) ?? '',
      };

      const encode = (obj: unknown) => btoa(JSON.stringify(obj));

      const mappedSession: Session = {
        capacity: session.capacity,
        slotLength: session.slotLength,
        services: session.services,
        startTime: {
          hour: form.newStartTime.split(':')[0],
          minute: form.newStartTime.split(':')[1],
        },
        endTime: {
          hour: session.until.split(':')[0],
          minute: session.until.split(':')[1],
        },
        break: 'no',
      };

      router.push(
        `/site/${site.id}/availability/edit/confirmation?session=${encode(existingSession)}&date=${date}&sessionToEdit=${encode(mappedSession)}`,
      );
    });
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

        {pendingSubmit ? (
          <SmallSpinnerWithText text="Working..." />
        ) : (
          <Button type="submit">Change session</Button>
        )}
      </form>
    </>
  );
};

export default EditSessionStartTimeForm;
