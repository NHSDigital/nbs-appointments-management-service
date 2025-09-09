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
  parseToUkDatetime,
} from '@services/timeService';
import { AvailabilitySession, SessionSummary, Site } from '@types';
import { notFound } from 'next/navigation';
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

  const requestedStartTime = parseToTimeComponents(updatedSession.from);
  const requestedEndTime = parseToTimeComponents(updatedSession.until);
  const existingStart = parseToUkDatetime(existingSession.ukStartDatetime);
  const existingEnd = parseToUkDatetime(existingSession.ukEndDatetime);

  const existingStartTimeComponent = parseToTimeComponents(
    `${existingStart.hour()}:${existingStart.minute()}`,
  );
  const existingEndTimeComponent = parseToTimeComponents(
    `${existingEnd.hour()}:${existingEnd.minute()}`,
  );

  if (
    requestedStartTime === undefined ||
    requestedEndTime === undefined ||
    existingStartTimeComponent === undefined ||
    existingEndTimeComponent === undefined
  ) {
    notFound(); // TODO: temp solution to stop build errors
  }

  const requestedStart = parseDateAndTimeComponentsToUkDateTime(
    date,
    requestedStartTime,
  );

  const nearestOptions = getNearestAlignedTimes(
    requestedStart,
    existingSession.slotLength,
  );

  const submitForm = async (form: EditSessionStartTimeFormValues) => {
    alert('Hello');
  };

  return (
    <>
      <p>
        There are booked appointments in this sessions which means the start
        time cannot be changed to: {requestedStart.format('HH:mma')}
      </p>
      <form>
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
                  value={`${option.format('HH:mma')}`}
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
