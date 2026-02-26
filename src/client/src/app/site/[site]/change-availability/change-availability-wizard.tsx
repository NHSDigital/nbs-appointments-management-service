'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition } from 'react';
import BeforeYouContinueStep from './before-you-continue-step';
import SelectDatesStep from './select-dates-step';
import CancellationImpactStep from './cancellation-impact-step';
import {
  createChangeAvailabilityFormSchema,
  ChangeAvailabilityFormValues,
} from './change-availability-form-schema';
import { yupResolver } from '@hookform/resolvers/yup';

interface Props {
  cancelADateRangeWithBookings: boolean;
  site: string;
}

const DEFAULT_MAX_CANCELLATION_DAYS = 90;

const ChangeAvailabilityWizard = ({
  cancelADateRangeWithBookings,
  site,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const methods = useForm<ChangeAvailabilityFormValues>({
    resolver: yupResolver(
      createChangeAvailabilityFormSchema(DEFAULT_MAX_CANCELLATION_DAYS),
    ),
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
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ChangeAvailabilityWizard;
