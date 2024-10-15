'use client';
import {
  BackLink,
  BackLinkClient,
  Button,
  DateInput,
  TextInput,
} from '@components/nhsuk-frontend';
import { useFormContext } from 'react-hook-form';
import { AvailabilityPeriodFormValues } from './availability-period-wizard';
import { InjectedWizardProps } from '@components/wizard';
// type Props = { userProfile: UserProfile } & InjectedWizardProps;

const StartAndEndDateStep = ({
  goToNextStep,
  goToPreviousStep,
  transitionToOnCancel,
}: InjectedWizardProps) => {
  const { register } = useFormContext<AvailabilityPeriodFormValues>();

  const onContinue = () => {
    // TODO: trigger field validation
    // if (validationPassed) {
    goToNextStep();
  };

  return (
    <>
      {transitionToOnCancel ? (
        <BackLink href={transitionToOnCancel} />
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
      <DateInput
        heading="Start date"
        hint="For example, 15 3 1984"
        id="date-of-birth-input"
      >
        <TextInput
          label="Day"
          id="date-of-birth-input-day"
          inputType="date"
          {...register('startDateDay')}
        />
        <TextInput
          label="Month"
          id="date-of-birth-input-month"
          inputType="date"
          {...register('startDateMonth')}
        />
        <TextInput
          label="Year"
          id="date-of-birth-input-year"
          inputType="date"
          {...register('startDateYear')}
        />
      </DateInput>
      <br />
      <DateInput
        heading="End date"
        hint="For example, 15 3 1984"
        id="date-of-birth-input"
      >
        <TextInput
          label="Day"
          id="date-of-birth-input-day"
          inputType="date"
          {...register('startDateDay')}
        />
        <TextInput
          label="Month"
          id="date-of-birth-input-month"
          inputType="date"
          {...register('startDateMonth')}
        />
        <TextInput
          label="Year"
          id="date-of-birth-input-year"
          inputType="date"
          {...register('startDateYear')}
        />
      </DateInput>
      <Button
        type="button"
        onClick={() => {
          onContinue();
        }}
      >
        Continue
      </Button>
    </>
  );
};

export default StartAndEndDateStep;
