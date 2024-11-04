/* eslint-disable react/jsx-props-no-spreading */
'use client';
import NhsHeading from '@components/nhs-heading';
import {
  BackLink,
  Button,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { InjectedWizardProps } from '@components/wizard';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { Controller, useFormContext } from 'react-hook-form';
import CapacityCalculation from './capacity-calculation';
import { formatTimeString } from '@services/timeService';

const TimeAndCapacityStep = ({
  goToNextStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
  setCurrentStep,
}: InjectedWizardProps) => {
  const { register, watch, formState, trigger, control, getValues } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const [startTimeWatch, endTimeWatch, slotLengthWatch, capacityWatch] = watch([
    'session.startTime',
    'session.endTime',
    'session.slotLength',
    'session.capacity',
  ]);

  const validateFields = async () => {
    return trigger([
      'session.startTime',
      'session.endTime',
      'session.capacity',
      'session.slotLength',
    ]);
  };

  const onContinue = async () => {
    const formIsValid = await validateFields();
    if (!formIsValid) {
      return;
    }

    goToNextStep();
  };

  const onBack = async () => {
    if (getValues('sessionType') === 'repeating') {
      goToPreviousStep();
    } else {
      setCurrentStep(stepNumber - 2);
    }
  };

  return (
    <>
      {stepNumber === 1 ? (
        <BackLink
          href={returnRouteUponCancellation ?? '/'}
          renderingStrategy="server"
        />
      ) : (
        <BackLink onClick={onBack} renderingStrategy="client" />
      )}
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
          errors.session?.startTime?.message || errors.session?.endTime?.message
        }
      >
        <Controller
          name={'session.startTime'}
          control={control}
          rules={{
            validate: value => {
              if (formatTimeString(value) === undefined) {
                return 'Enter a valid start time';
              }
            },
          }}
          render={() => (
            <>
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
                    {...register('session.startTime.hour', {
                      valueAsNumber: true,
                    })}
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
                    {...register('session.startTime.minute', {
                      valueAsNumber: true,
                    })}
                  ></input>
                </div>
              </div>
            </>
          )}
        />
        <Controller
          name={'session.endTime'}
          control={control}
          rules={{
            validate: (value, form) => {
              const endTime = formatTimeString(value);
              const startTime = formatTimeString(form.session.startTime);
              if (endTime === undefined || startTime === undefined) {
                return 'Enter a valid end time';
              }

              if (
                form.session.startTime.hour > value.hour ||
                (form.session.startTime.hour === value.hour &&
                  form.session.startTime.minute > value.minute)
              ) {
                return 'End time cannot be earlier than start time';
              }
            },
          }}
          render={() => (
            <>
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
                    {...register('session.endTime.hour', {
                      valueAsNumber: true,
                    })}
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
                    {...register('session.endTime.minute', {
                      valueAsNumber: true,
                    })}
                  ></input>
                </div>
              </div>
            </>
          )}
        />
      </FormGroup>
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
              valueAsNumber: true,
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
              valueAsNumber: true,
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
