'use client';
import Wizard from '@components/wizard';
import WizardStep from '@components/wizard-step';
import fromServer from '@server/fromServer';
import { cancelDay } from '@services/appointmentsService';
import { parseToUkDatetime, RFC3339Format } from '@services/timeService';
import { CancelDayRequest, ClinicalService, DaySummaryV2, Site } from '@types';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import CancelChoiceStep from './wizard-steps/cancel-choice-step';
import { ConfirmCancelChoiceStep } from './wizard-steps/confirm-cancel-choice-step';

type Props = {
  date: string;
  site: Site;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

export type CancelDayFromValues = {
  cancelChoice: 'true' | 'false' | undefined;
};

const CancelDayWizard = ({
  date,
  site,
  daySummary,
  clinicalServices,
}: Props) => {
  const [pendingSubmit, startTransition] = useTransition();
  const methods = useForm<CancelDayFromValues>({
    defaultValues: {
      cancelChoice: undefined,
    },
  });
  const { replace } = useRouter();
  const parsedDate = parseToUkDatetime(date);

  const submitForm = async () => {
    startTransition(async () => {
      const payload: CancelDayRequest = {
        site: site.id,
        date: parsedDate.format(RFC3339Format),
      };

      const response = await fromServer(cancelDay(payload));
      replace(
        `/site/${site.id}/cancel-day/confirmed?date=${date}&cancelledBookingCount=${response.cancelledBookingCount}&bookingsWithoutContactDetails=${response.bookingsWithoutContactDetails}`,
      );
    });
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>
        <Wizard
          id="cancel-day-wizard"
          initialStep={1}
          returnRouteUponCancellation={`/site/${site.id}/view-availability/week?date=${date}`}
          onCompleteFinalStep={() => {
            methods.handleSubmit(submitForm);
          }}
          pendingSubmit={pendingSubmit}
        >
          <WizardStep>
            {stepProps => (
              <CancelChoiceStep
                {...stepProps}
                clinicalServices={clinicalServices}
                date={parsedDate}
                daySummary={daySummary}
                site={site}
              />
            )}
          </WizardStep>

          <WizardStep>
            {stepProps => (
              <ConfirmCancelChoiceStep
                {...stepProps}
                clinicalServices={clinicalServices}
                date={parsedDate}
                daySummary={daySummary}
                site={site}
              />
            )}
          </WizardStep>
        </Wizard>
      </form>
    </FormProvider>
  );
};

export default CancelDayWizard;
