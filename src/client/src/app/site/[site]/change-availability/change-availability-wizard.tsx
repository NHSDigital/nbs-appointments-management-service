'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import BeforeYouContinueStep from './before-you-continue-step';

interface ChangeAvailabilityFormValues {
  dateFrom: string;
}
interface Props {
  cancelADateRangeWithBookings: boolean;
}

const ChangeAvailabilityWizard = ({ cancelADateRangeWithBookings }: Props) => {
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
            {stepProps => (
              <BeforeYouContinueStep
                {...stepProps}
                cancelADateRangeWithBookingsEnabled={
                  cancelADateRangeWithBookings
                }
              />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ChangeAvailabilityWizard;
