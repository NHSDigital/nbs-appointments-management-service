'use client';
import {
  BackLink,
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { AvailabilityPeriodFormValues } from './availability-period-wizard';
import { InjectedWizardProps } from '@components/wizard';
import dayjs from 'dayjs';
import {
  isSameDayOrBefore,
  parseAndValidateDateFromComponents,
} from '@services/timeService';
import NhsHeading from '@components/nhs-heading';

const StartAndEndDateStep = ({
  goToNextStep,
  goToPreviousStep,
  returnRouteUponCancellation,
}: InjectedWizardProps) => {
  const { register, formState, trigger, watch, setError } =
    useFormContext<AvailabilityPeriodFormValues>();
  const { errors } = formState;

  const {
    startDateDay,
    startDateMonth,
    startDateYear,
    endDateDay,
    endDateMonth,
    endDateYear,
  } = watch();

  const validateFields = async () => {
    const fieldsAreIndividuallyValid = await trigger([
      'startDateDay',
      'startDateMonth',
      'startDateYear',
      'endDateDay',
      'endDateMonth',
      'endDateYear',
    ]);
    if (!fieldsAreIndividuallyValid) {
      return false;
    }

    const startDate = parseAndValidateDateFromComponents(
      startDateDay,
      startDateMonth,
      startDateYear,
    );
    if (startDate === undefined) {
      setError('startDateDay', { message: 'Please enter a valid date' });
      return false;
    }

    const endDate = parseAndValidateDateFromComponents(
      endDateDay,
      endDateMonth,
      endDateYear,
    );
    if (endDate === undefined) {
      setError('endDateDay', { message: 'Please enter a valid date' });
      return false;
    }

    const endDateIsAfterStartDate = isSameDayOrBefore(startDate, endDate);
    if (!endDateIsAfterStartDate) {
      setError('endDateDay', {
        message: 'End date must be after the start date',
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
      <BackLink
        href={returnRouteUponCancellation ?? ''}
        onClick={returnRouteUponCancellation ? undefined : goToPreviousStep}
      />
      <NhsHeading
        title="Add start and end dates for your availability period"
        caption="Create availability period"
      />
      <FormGroup
        error={
          errors.startDateDay?.message ||
          errors.startDateMonth?.message ||
          errors.startDateYear?.message
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
            {...register('startDateDay', {
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
            {...register('startDateMonth', {
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
            {...register('startDateYear', {
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
      <br />
      <FormGroup
        error={
          errors.endDateDay?.message ||
          errors.endDateMonth?.message ||
          errors.endDateYear?.message
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
            {...register('endDateDay', {
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
            {...register('endDateMonth', {
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
            {...register('endDateYear', {
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
