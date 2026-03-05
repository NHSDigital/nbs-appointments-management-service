'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import BeforeYouContinueStep from './before-you-continue-step';
import SelectDatesStep from './select-dates-step';
import CancellationImpactStep from './cancellation-impact-step';
import CheckYourAnswersStep from './check-your-answers-step';
import ConfirmationStep from './confirmation-step';
import {
  createChangeAvailabilityFormSchema,
  ChangeAvailabilityFormValues,
} from './change-availability-form-schema';
import { yupResolver } from '@hookform/resolvers/yup';

interface Props {
  cancelADateRangeWithBookings: boolean;
  site: string;
  rangeMaximumDays: number;
}

const ChangeAvailabilityWizard = ({
  cancelADateRangeWithBookings,
  site,
  rangeMaximumDays,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const methods = useForm<ChangeAvailabilityFormValues>({
    resolver: yupResolver(createChangeAvailabilityFormSchema(rangeMaximumDays)),
    defaultValues: {
      startDate: { day: '', month: '', year: '' },
      endDate: { day: '', month: '', year: '' },
    },
  });

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
          <WizardStep>
            {stepProps => <SelectDatesStep {...stepProps} site={site} />}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <CancellationImpactStep
                {...stepProps}
                cancelADateRangeWithBookingsEnabled={
                  cancelADateRangeWithBookings
                }
                site={site}
              />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => <CheckYourAnswersStep {...stepProps} site={site} />}
          </WizardStep>
          <WizardStep>
            {stepProps => <ConfirmationStep {...stepProps} site={site} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ChangeAvailabilityWizard;
