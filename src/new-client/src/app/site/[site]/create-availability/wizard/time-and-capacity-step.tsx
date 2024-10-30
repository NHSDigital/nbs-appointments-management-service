/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import { Button, FormGroup, TextInput } from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { useFormContext } from 'react-hook-form';
import CapacityCalculation from './capacity-calculation';
import { formatTimeString } from '@services/timeService';

const TimeAndCapacityStep = ({ goToNextStep }: InjectedWizardProps) => {
  const { register, watch, formState, trigger, setError } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const [startTimeWatch, endTimeWatch, slotLengthWatch, capacityWatch] = watch([
    'session.startTime',
    'session.endTime',
    'session.slotLength',
    'session.capacity',
  ]);

  const validateFields = async () => {
    const fieldsAreValid = await trigger([
      'session.capacity',
      'session.slotLength',
    ]);
    if (!fieldsAreValid) {
      return;
    }

    const parsedStart = formatTimeString(startTimeWatch);
    if (parsedStart === undefined) {
      setError('session.startTime', { message: 'Enter a valid start time' });
      return false;
    }

    const parsedEnd = formatTimeString(endTimeWatch);
    if (parsedEnd === undefined) {
      setError('session.endTime', {
        message: 'Enter a valid end time',
      });
      return false;
    }

    if (
      startTimeWatch.hour > endTimeWatch.hour ||
      (startTimeWatch.hour === endTimeWatch.hour &&
        startTimeWatch.minute > endTimeWatch.minute)
    ) {
      setError('session.startTime', {
        message: 'End time cannot be earlier than start time',
      });
      return false;
    }

    return true;
  };

  const onContinue = async () => {
    const formIsValid = await validateFields();

    if (!formIsValid) {
      return;
    }

    goToNextStep();
  };

  return (
    <>
      <NhsHeading
        title="Set time and capacity for your session"
        caption="Create availability period"
      />

      <p>
        You can add multiple sessions to this availability period, to cover part
        time work or different service types.
      </p>

      {/* TODO: Create nhsuk-frontend components for these time controls */}
      <FormGroup
        legend="Session times"
        hint="For example, 14:30"
        error={
          errors.session?.startTime?.hour?.message ||
          errors.session?.startTime?.minute?.message ||
          errors.session?.endTime?.hour?.message ||
          errors.session?.endTime?.minute?.message ||
          errors.session?.startTime?.message ||
          errors.session?.endTime?.message
        }
      >
        <div className="nhsuk-label">Start time</div>
        <div className="nhsuk-time-input-custom">
          <div className="nhsuk-time-input-custom__item">
            <label
              id="start-time-accessibility-label-hour"
              htmlFor="start-time-hour"
            >
              Session start time - hour
            </label>
            <input
              aria-labelledby="start-time-accessibility-label-hour"
              id="start-time-hour"
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.startTime.hour')}
            ></input>
          </div>
          <div className="nhsuk-time-input-custom__item">
            <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
              :
            </div>
          </div>

          <div className="nhsuk-time-input-custom__item">
            <label
              id="start-time-accessibility-label-minute"
              htmlFor="start-time-minute"
            >
              Session start time - minute
            </label>
            <input
              aria-labelledby="start-time-accessibility-label-minute"
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.startTime.minute')}
            ></input>
          </div>
        </div>

        <div className="nhsuk-label">End time</div>
        <div className="nhsuk-time-input-custom">
          <div className="nhsuk-time-input-custom__item">
            <label
              id="end-time-accessibility-label-hour"
              htmlFor="end-time-hour"
            >
              Session end time - hour
            </label>
            <input
              aria-labelledby="end-time-accessibility-label-hour"
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.endTime.hour')}
            ></input>
          </div>
          <div className="nhsuk-time-input-custom__item">
            <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
              :
            </div>
          </div>

          <div className="nhsuk-time-input-custom__item">
            <label
              id="end-time-accessibility-label-minute"
              htmlFor="end-time-minute"
            >
              Session end time - minute
            </label>
            <input
              aria-labelledby="end-time-accessibility-label-minute"
              className="nhsuk-input nhsuk-time-input-custom__input nhsuk-input--width-2"
              {...register('session.endTime.minute')}
            ></input>
          </div>
        </div>
      </FormGroup>

      {/* <br />
      <FormGroup legend="Add a break to this session?">
        <RadioGroup>
          <Radio
            label="Yes"
            {...register('session.break')}
            id="break-yes"
            value="yes"
          />
          <Radio
            label="No"
            {...register('session.break')}
            id="break-no"
            value="no"
          />
        </RadioGroup>
      </FormGroup> */}

      <br />
      <FormGroup
        legend="Capacity"
        hint="Enter your capacity to calculate appointment numbers for this session"
      >
        <FormGroup
          legend="How many vaccinators or spaces do you have?"
          error={errors.session?.capacity?.message}
        >
          <label id="capacity" htmlFor="capacity" style={{ display: 'none' }}>
            How many vaccinators or spaces do you have?
          </label>
          <TextInput
            {...register('session.capacity', {
              required: { value: true, message: 'Capacity is required' },
              min: { value: 1, message: 'Capacity must be at least 1' },
              validate: value => {
                if (!Number.isInteger(Number(value))) {
                  return 'Capacity must be a whole number.';
                }
              },
            })}
            id="capacity"
            aria-labelledby="capacity"
            inputMode="numeric"
            width={2}
          ></TextInput>
        </FormGroup>

        <FormGroup
          legend="How long are your appointments?"
          error={errors.session?.slotLength?.message}
        >
          <label
            id="slot-length"
            htmlFor="slot-length"
            style={{ display: 'none' }}
          >
            How long are your appointments?
          </label>
          <TextInput
            {...register('session.slotLength', {
              required: {
                value: true,
                message: 'Appointment length is required',
              },
              min: {
                value: 1,
                message: 'Appointment length must be at least 1 minute',
              },
              validate: value => {
                if (!Number.isInteger(Number(value))) {
                  return 'Appointment length must be a whole number.';
                }
              },
            })}
            id="slot-length"
            aria-labelledby="slot-length"
            inputMode="numeric"
            width={2}
            suffix="minutes"
          ></TextInput>
        </FormGroup>
      </FormGroup>

      <CapacityCalculation
        startTime={startTimeWatch}
        endTime={endTimeWatch}
        slotLength={slotLengthWatch}
        capacity={capacityWatch}
      />
      <Button
        type="button"
        onClick={async () => {
          await onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default TimeAndCapacityStep;
