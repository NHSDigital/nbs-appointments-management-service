'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import StartAndEndDateStep from './start-and-end-date-step';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { AvailabilityPeriod, Site, UserProfile } from '@types';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import saveAvailabilityPeriod from './save-availability-period';

export type AvailabilityPeriodFormValues = {
  startDateDay: number;
  startDateMonth: number;
  startDateYear: number;
  endDateDay: number;
  endDateMonth: number;
  endDateYear: number;
};

type AvailabilityPeriodWizardState = {
  initialStep: number;
  activeStep: number;
};

type Props = {
  site: Site;
};

const AvailabilityPeriodWizard = ({ site }: Props) => {
  //   const wizardState = useState<AvailabilityPeriodWizardState>({
  //     initialStep: 1,
  //     activeStep: 1,
  //   });
  const methods = useForm<AvailabilityPeriodFormValues>();

  const submitForm: SubmitHandler<AvailabilityPeriodFormValues> = (
    form: AvailabilityPeriodFormValues,
  ) => {
    const availabilityPeriod = mapFormValuesToAvailabilityPeriod(form);
    saveAvailabilityPeriod(availabilityPeriod);
  };

  const { replace } = useRouter();
  const cancel = () => {
    replace(`/site/${site}/create-availability`);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        {/* <StartAndEndDateStep
          userProfile={userProfile}
          stepNumber={0}
          currentStep={0}
          isActive={false}
          setCurrentStep={function (step: number): void {
            throw new Error('Function not implemented.');
          }}
          goToNextStep={function (): void {
            throw new Error('Function not implemented.');
          }}
          goToPreviousStep={function (): void {
            throw new Error('Function not implemented.');
          }}
        /> */}
        <Wizard initialStep={1} id="create-availability-wizard">
          <WizardStep onBack={cancel}>
            {stepProps => <StartAndEndDateStep {...stepProps} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

const mapFormValuesToAvailabilityPeriod = (
  formValues: AvailabilityPeriodFormValues,
): AvailabilityPeriod => {
  // TODO: Implement this properly and use DayJS, plus validation etc. etc.
  return {
    startDate: new Date(
      formValues.startDateYear,
      formValues.startDateMonth,
      formValues.startDateDay,
    ),
    endDate: new Date(
      formValues.endDateYear,
      formValues.endDateMonth,
      formValues.endDateDay,
    ),
    services: [],
    status: 'Unpublished',
  };
};

export default AvailabilityPeriodWizard;
