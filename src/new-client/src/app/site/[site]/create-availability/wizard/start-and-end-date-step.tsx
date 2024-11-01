'use client';
import {
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { CreateAvailabilityFormValues } from './availability-template-wizard';
import { InjectedWizardProps } from '@components/wizard';
import dayjs from 'dayjs';
import { isSameDayOrBefore, parseDateComponents } from '@services/timeService';
import NhsHeading from '@components/nhs-heading';

const StartAndEndDateStep = ({
  stepNumber,
  goToNextStep,
  setCurrentStep,
}: InjectedWizardProps) => {
  const { register, formState, trigger, watch, setError, getValues } =
    useFormContext<CreateAvailabilityFormValues>();
  const { errors } = formState;

  const [startDateWatch, endDateWatch] = watch(['startDate', 'endDate']);
  const sessionType = getValues('sessionType');

  const validateFields = async () => {
    const fieldsAreIndividuallyValid = await trigger([
      'startDate.day',
      'startDate.month',
      'startDate.year',
      'endDate.day',
      'endDate.month',
      'endDate.year',
    ]);
    if (!fieldsAreIndividuallyValid) {
      return false;
    }

    const startDate = parseDateComponents(startDateWatch);
    if (startDate === undefined) {
      setError('startDate.day', { message: 'Please enter a valid date' });
      return false;
    }

    if (sessionType === 'repeating') {
      const endDate = parseDateComponents(
        endDateWatch ?? { day: 0, month: 0, year: 0 },
      );

      if (endDate === undefined) {
        setError('endDate.day', { message: 'Please enter a valid date' });
        return false;
      }

      const endDateIsAfterStartDate = isSameDayOrBefore(startDate, endDate);
      if (!endDateIsAfterStartDate) {
        setError('endDate', {
          message: 'End date must be after the start date',
        });
        return false;
      }
    }

    return true;
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
      <FormGroup
        error={
          errors.startDate?.message ||
          errors.startDate?.day?.message ||
          errors.startDate?.month?.message ||
          errors.startDate?.year?.message
        }
      >
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
              required: 'Day is required',
              min: { value: 1, message: 'Please enter a valid day' },
              max: { value: 31, message: 'Please enter a valid day' },
              pattern: {
                value: /^[0-9]*$/,
                message: 'Please enter a valid day',
              },
            })}
          />
          <TextInput
            label="Month"
            type="number"
            id="start-date-input-month"
            inputType="date"
            {...register('startDate.month', {
              required: 'Month is required',
              min: { value: 1, message: 'Please enter a valid month' },
              max: { value: 12, message: 'Please enter a valid month' },
              pattern: {
                value: /^[0-9]*$/,
                message: 'Please enter a valid month',
              },
            })}
          />
          <TextInput
            label="Year"
            type="number"
            id="start-date-input-year"
            inputType="date"
            {...register('startDate.year', {
              required: 'Year is required',
              min: {
                value: dayjs().year(),
                message: 'Please enter a valid year',
              },
              max: { value: 3000, message: 'Please enter a valid year' },
              pattern: {
                value: /^[0-9]*$/,
                message: 'Please enter a valid year',
              },
            })}
          />
        </DateInput>
      </FormGroup>
      {getValues('sessionType') === 'repeating' && (
        <>
          {' '}
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
                  required: 'Day is required',
                  min: { value: 1, message: 'Please enter a valid day' },
                  max: { value: 31, message: 'Please enter a valid day' },
                  pattern: {
                    value: /^[0-9]*$/,
                    message: 'Please enter a valid day',
                  },
                })}
              />
              <TextInput
                label="Month"
                type="number"
                id="end-date-input-month"
                inputType="date"
                {...register('endDate.month', {
                  required: 'Month is required',
                  min: { value: 1, message: 'Please enter a valid month' },
                  max: { value: 12, message: 'Please enter a valid month' },
                  pattern: {
                    value: /^[0-9]*$/,
                    message: 'Please enter a valid month',
                  },
                })}
              />
              <TextInput
                label="Year"
                type="number"
                id="end-date-input-year"
                inputType="date"
                {...register('endDate.year', {
                  required: 'Year is required',
                  min: { value: 2020, message: 'Please enter a valid year' },
                  max: { value: 3000, message: 'Please enter a valid year' },
                  pattern: {
                    value: /^[0-9]*$/,
                    message: 'Please enter a valid year',
                  },
                })}
              />
            </DateInput>
          </FormGroup>
        </>
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
