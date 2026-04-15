'use client';
import { FormProvider, SubmitHandler, useForm } from 'react-hook-form';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import { useTransition, useEffect, useState } from 'react';
import BeforeYouContinueStep from './before-you-continue-step';
import SelectDatesStep from './select-dates-step';
import CancellationImpactStep from './cancellation-impact-step';
import CheckYourAnswersStep from './check-your-answers-step';
import ConfirmationStep from './confirmation-step';
import NoNotificationStep from './no-notification-step';
import {
  createChangeAvailabilityFormSchema,
  ChangeAvailabilityFormValues,
} from './change-availability-form-schema';
import { yupResolver } from '@hookform/resolvers/yup';
import { useSearchParams } from 'next/navigation';

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
  const [originUrl, setOriginUrl] = useState<string | undefined>(undefined);

  const searchParams = useSearchParams();

  useEffect(() => {
    const returnUrl = searchParams.get('returnUrl');
    const ref = document.referrer;

    // Only "lock in" the back destination if the user actually navigated
    // from within MYA.
    const isInternalNavigation = ref && ref.includes(window.location.host);

    if (returnUrl && isInternalNavigation) {
      setOriginUrl(returnUrl);
    } else {
      // If they pasted the link or came from an external link like Google, we keep originUrl undefined
      // so that handleBack defaults to '/sites'
      setOriginUrl(undefined);
    }
  }, [searchParams]);

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
          returnRouteUponCancellation={`/manage-your-appointments/sites`}
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
                previousUrl={originUrl}
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
          <WizardStep>
            {stepProps => <NoNotificationStep {...stepProps} site={site} />}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default ChangeAvailabilityWizard;
