'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import BeforeYouContinueStep from './before-you-continue-step';

interface ChangeAvailabilityFormValues {
  dateFrom: string;
}

const ChangeAvailabilityWizard = () => {
  const [pendingSubmit, startTransition] = useTransition();
  const methods = useForm<ChangeAvailabilityFormValues>({});
  const submitForm: SubmitHandler<ChangeAvailabilityFormValues> = async () => {
    startTransition(async () => {});
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="change-availability-wizard"
          initialStep={1}
          returnRouteUponCancellation={`/todo`}
          onCompleteFinalStep={() => {}}
          pendingSubmit={pendingSubmit}
        >
          <WizardStep>
            {stepProps => <BeforeYouContinueStep {...stepProps} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ChangeAvailabilityWizard;
