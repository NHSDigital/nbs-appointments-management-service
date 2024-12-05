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
import {
  CreateAvailabilityFormValues,
  handlePositiveBoundedNumberInput,
} from './availability-template-wizard';
import { Controller, useFormContext } from 'react-hook-form';
import CapacityCalculation, {
  sessionLengthInMinutes,
} from './capacity-calculation';
import { formatTimeString } from '@services/timeService';
import { ChangeEvent } from 'react';

const TimeAndCapacityStep = ({
  goToNextStep,
  goToLastStep,
  stepNumber,
  returnRouteUponCancellation,
  goToPreviousStep,
  setCurrentStep,
}: InjectedWizardProps) => {
  const { watch, formState, trigger, control, getValues } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors, isValid: allStepsAreValid, touchedFields } = formState;

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

  const shouldSkipToSummaryStep =
    touchedFields.session?.services && allStepsAreValid;

  const onContinue = async () => {
    const formIsValid = await validateFields();
    if (!formIsValid) {
      return;
    }

    if (shouldSkipToSummaryStep) {
      goToLastStep();
    } else {
      goToNextStep();
    }
  };

  const onBack = async () => {
    if (getValues('sessionType') === 'repeating') {
      goToPreviousStep();
    } else {
      setCurrentStep(stepNumber - 2);
    }
  };

  const handleTwoDigitPositiveBoundedNumberInput = (
    e: ChangeEvent<HTMLInputElement>,
    upperBound: number,
  ) => {
    const asNumber = Number(e.currentTarget.value);
    if (asNumber < 0 || Number.isNaN(asNumber) || !Number.isInteger(asNumber)) {
      return '00';
    }

    if (asNumber > upperBound) {
      return `0${e.target.value.slice(-1)}`;
    }

    switch (e.target.value.length) {
      case 0:
        return `00`;
      case 1:
        return `0${e.target.value}`;
      case 2:
        return e.target.value;
      default:
        return e.target.value.slice(-2);
    }
  };

  const sessionType = getValues('sessionType');

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
        caption={
          sessionType === 'single'
            ? 'Create single date session'
            : 'Create weekly session'
        }
      />

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
                <Controller
                  control={control}
                  name="session.startTime.hour"
                  render={({ field }) => (
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
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 23),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />

                <div className="nhsuk-time-input-custom__item">
                  <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
                    :
                  </div>
                </div>

                <Controller
                  control={control}
                  name="session.startTime.minute"
                  render={({ field }) => (
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
                        id="start-time-minute"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 59),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />
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

              const minutesBetweenStartAndEnd = sessionLengthInMinutes(
                form.session.startTime,
                value,
              );

              if (minutesBetweenStartAndEnd < 0) {
                return 'Session end time must be after the start time';
              }

              if (minutesBetweenStartAndEnd <= 5) {
                return 'Session length must be more than 5 minutes';
              }
            },
          }}
          render={() => (
            <>
              <div className="nhsuk-label">End time</div>
              <div className="nhsuk-time-input-custom">
                <Controller
                  control={control}
                  name="session.endTime.hour"
                  render={({ field }) => (
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
                        id="end-time-hour"
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 23),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />

                <div className="nhsuk-time-input-custom__item">
                  <div style={{ display: 'inline-block', fontSize: 'x-large' }}>
                    :
                  </div>
                </div>

                <Controller
                  control={control}
                  name="session.endTime.minute"
                  render={({ field }) => (
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
                        onChange={e =>
                          field.onChange(
                            handleTwoDigitPositiveBoundedNumberInput(e, 59),
                          )
                        }
                        value={field.value ?? ''}
                      ></input>
                    </div>
                  )}
                />
              </div>
            </>
          )}
        />
      </FormGroup>
      <br />
      <FormGroup
        legend="Capacity"
        hint="Enter your capacity to calculate appointment numbers for this session."
      >
        <Controller
          control={control}
          name="session.capacity"
          rules={{
            required: { value: true, message: 'Capacity is required' },
            min: { value: 1, message: 'Capacity must be at least 1' },
            validate: value => {
              if (!Number.isInteger(Number(value))) {
                return 'Capacity must be a whole number';
              }
            },
          }}
          render={({ field }) => (
            <FormGroup
              legend="How many vaccinators or vaccination spaces do you have?"
              error={errors.session?.capacity?.message}
            >
              <label
                id="capacity"
                htmlFor="capacity"
                style={{ display: 'none' }}
              >
                How many vaccinators or vaccination spaces do you have?
              </label>
              <TextInput
                id="capacity"
                aria-labelledby="capacity"
                inputMode="numeric"
                type="number"
                width={2}
                onChange={e =>
                  field.onChange(handlePositiveBoundedNumberInput(e, 99))
                }
                value={field.value ?? ''}
              />
            </FormGroup>
          )}
        />

        <Controller
          control={control}
          name="session.slotLength"
          rules={{
            required: {
              value: true,
              message: 'Appointment length is required',
            },
            min: {
              value: 1,
              message: 'Appointment length must be at least 1 minute',
            },
            max: {
              value: 60,
              message: 'Appointment length cannot exceed 1 hour',
            },
            validate: (value, form) => {
              if (!Number.isInteger(Number(value))) {
                return 'Appointment length must be a whole number';
              }

              const minutesInSession = sessionLengthInMinutes(
                form.session.startTime,
                form.session.endTime,
              );
              const shouldSkipApptShorterThanSessionValidation =
                Number.isNaN(minutesInSession) || minutesInSession <= 0;

              if (
                !shouldSkipApptShorterThanSessionValidation &&
                value > minutesInSession
              ) {
                return 'Appointment length must be shorter than session length';
              }
            },
          }}
          render={({ field }) => (
            <FormGroup
              legend="How long are your appointments?"
              error={errors.session?.slotLength?.message}
              hint="Appointment length must be a maximum of 60 minutes or less."
            >
              <label
                id="slot-length"
                htmlFor="slot-length"
                style={{ display: 'none' }}
              >
                How long are your appointments?
              </label>
              <TextInput
                id="slot-length"
                aria-labelledby="slot-length"
                inputMode="numeric"
                type="number"
                width={2}
                suffix="minutes"
                onChange={e => {
                  field.onChange(handlePositiveBoundedNumberInput(e, 60));
                }}
                value={field.value ?? ''}
              />
            </FormGroup>
          )}
        />
      </FormGroup>

      <CapacityCalculation
        startTime={{
          hour: Number(startTimeWatch?.hour),
          minute: Number(startTimeWatch?.minute),
        }}
        endTime={{
          hour: Number(endTimeWatch?.hour),
          minute: Number(endTimeWatch?.minute),
        }}
        slotLength={Number(slotLengthWatch)}
        capacity={Number(capacityWatch)}
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
