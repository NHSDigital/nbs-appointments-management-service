'use client';
import {
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { Controller, useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import dayjs from 'dayjs';
import {
  isSameDayOrBefore,
  now,
  parseDateComponents,
} from '@services/timeService';
import NhsHeading from '@components/nhs-heading';

const StartAndEndDateStep = ({
  stepNumber,
  goToNextStep,
  setCurrentStep,
}: InjectedWizardProps) => {
  const { register, formState, trigger, getValues, control } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const sessionType = getValues('sessionType');

  const validateFields = async () => {
    if (sessionType === 'single') {
      return trigger(['startDate']);
    } else {
      return trigger(['startDate', 'endDate']);
    }
  };

  const onContinue = async () => {
    const formIsValid = await validateFields();
    if (!formIsValid) {
      return;
    }

    if (getValues('sessionType') === 'repeating') {
      goToNextStep();
    } else {
      setCurrentStep(stepNumber + 2);
    }
  };

  return (
    <>
      <NhsHeading
        title={
          sessionType === 'single'
            ? 'Session date'
            : 'Add start and end dates for your availability period'
        }
        caption="Create availability period"
      />
      <FormGroup error={errors.startDate?.message}>
        <Controller
          name="startDate"
          control={control}
          rules={{
            validate: value => {
              const startDate = parseDateComponents(value);

              if (startDate === undefined) {
                return 'Session date must include a valid date';
              }

              if (startDate.isBefore(now(), 'day')) {
                return 'Session date must be in the future';
              }
            },
          }}
          render={() => (
            <DateInput
              heading="Start date"
              hint="For example, 15 3 1984"
              id="start-date-input"
            >
              <TextInput
                label="Day"
                type="number"
                id="start-date-input-day"
                inputType="date"
                {...register('startDate.day', {
                  valueAsNumber: true,
                  required: 'Session date must include a day',
                  min: {
                    value: 1,
                    message: 'Session date must include a valid day',
                  },
                  max: {
                    value: 31,
                    message: 'Session date must include a valid day',
                  },
                })}
              />
              <TextInput
                label="Month"
                type="number"
                id="start-date-input-month"
                inputType="date"
                {...register('startDate.month', {
                  valueAsNumber: true,
                  required: 'Session date must include a month',
                  min: {
                    value: 1,
                    message: 'Session date must include a valid month',
                  },
                  max: {
                    value: 12,
                    message: 'Session date must include a valid month',
                  },
                })}
              />
              <TextInput
                label="Year"
                type="number"
                id="start-date-input-year"
                inputType="date"
                {...register('startDate.year', {
                  valueAsNumber: true,
                  required: 'Session date must include a year',
                  min: {
                    value: now().year(),
                    message: 'Session date must be this year or a future year',
                  },
                  max: {
                    value: now().year() + 1,
                    message: 'Session date must include a valid year',
                  },
                })}
              />
            </DateInput>
          )}
        />
      </FormGroup>
      {getValues('sessionType') === 'repeating' && (
        <Controller
          name="endDate"
          control={control}
          rules={{
            validate: (value, form) => {
              const endDate = parseDateComponents(value);
              const startDate = parseDateComponents(form.startDate);

              if (endDate === undefined || startDate === undefined) {
                return 'Session end date must include a valid day';
              }

              if (!isSameDayOrBefore(startDate, endDate)) {
                return 'End date must be after the start date';
              }
            },
          }}
          render={() => (
            <>
              <br />
              <FormGroup
                error={
                  errors.endDate?.message ||
                  errors.endDate?.day?.message ||
                  errors.endDate?.month?.message ||
                  errors.endDate?.year?.message
                }
              >
                <DateInput
                  heading="End date"
                  hint="For example, 15 3 1984"
                  id="end-date-input"
                >
                  <TextInput
                    label="Day"
                    type="number"
                    id="end-date-input-day"
                    inputType="date"
                    {...register('endDate.day', {
                      valueAsNumber: true,
                      required: 'Day is required',
                      min: { value: 1, message: 'Please enter a valid day' },
                      max: { value: 31, message: 'Please enter a valid day' },
                    })}
                  />
                  <TextInput
                    label="Month"
                    type="number"
                    id="end-date-input-month"
                    inputType="date"
                    {...register('endDate.month', {
                      valueAsNumber: true,
                      required: 'Month is required',
                      min: { value: 1, message: 'Please enter a valid month' },
                      max: { value: 12, message: 'Please enter a valid month' },
                    })}
                  />
                  <TextInput
                    label="Year"
                    type="number"
                    id="end-date-input-year"
                    inputType="date"
                    {...register('endDate.year', {
                      valueAsNumber: true,
                      required: 'Year is required',
                      min: {
                        value: dayjs().utc().year(),
                        message: 'Please enter a valid year',
                      },
                      max: {
                        value: 3000,
                        message: 'Please enter a valid year',
                      },
                    })}
                  />
                </DateInput>
              </FormGroup>
            </>
          )}
        ></Controller>
      )}

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

export default StartAndEndDateStep;
