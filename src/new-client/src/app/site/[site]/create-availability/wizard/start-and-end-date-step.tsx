'use client';
import {
  BackLink,
  BackLinkClient,
  Button,
  DateInput,
  FormGroup,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { AvailabilityPeriodFormValues } from './availability-period-wizard';
import { InjectedWizardProps } from '@components/wizard';
import dayjs from 'dayjs';
// type Props = { userProfile: UserProfile } & InjectedWizardProps;

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

  const onContinue = async () => {
    const result = await trigger([
      'startDateDay',
      'startDateMonth',
      'startDateYear',
      'endDateDay',
      'endDateMonth',
      'endDateYear',
    ]);
    if (!result) {
      return;
    }

    console.dir({
      startDateDay,
      startDateMonth,
      startDateYear,
      endDateDay,
      endDateMonth,
      endDateYear,
    });
    const potentialStartDate = dayjs(
      new Date(startDateYear, Number(startDateMonth), startDateDay),
    );
    if (!potentialStartDate.isValid()) {
      setError('startDateDay', { message: 'Please enter a valid date' });
      return;
    }

    const potentialEndDate = dayjs(
      new Date(endDateYear, endDateMonth - 1, endDateDay),
    );
    if (!potentialEndDate.isValid()) {
      setError('endDateDay', { message: 'Please enter a valid date' });
      return;
    }

    //goToNextStep();
  };

  return (
    <>
      {returnRouteUponCancellation ? (
        <BackLink href={returnRouteUponCancellation} />
      ) : (
        <BackLinkClient
          onClick={() => {
            goToPreviousStep();
          }}
        />
      )}
      <h1 className="app-page-heading">
        <span className="nhsuk-caption-l">Create availability period</span>
        Add start and end dates for your availability period
      </h1>
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
          id="date-of-birth-input"
        >
          <TextInput
            label="Day"
            id="date-of-birth-input-day"
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
            id="date-of-birth-input-month"
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
            id="date-of-birth-input-year"
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
          id="date-of-birth-input"
        >
          <TextInput
            label="Day"
            type="number"
            id="date-of-birth-input-day"
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
            id="date-of-birth-input-month"
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
            id="date-of-birth-input-year"
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
