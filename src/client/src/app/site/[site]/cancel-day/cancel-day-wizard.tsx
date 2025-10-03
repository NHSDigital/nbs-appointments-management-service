'use client';
import { InsetText } from '@components/nhsuk-frontend';
import { SessionSummaryTable } from '@components/session-summary-table';
import fromServer from '@server/fromServer';
import { cancelDay } from '@services/appointmentsService';
import { parseToUkDatetime, RFC3339Format } from '@services/timeService';
import { CancelDayRequest, ClinicalService, DaySummaryV2 } from '@types';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';
import { FormProvider, useForm } from 'react-hook-form';

type Props = {
  date: string;
  siteId: string;
  daySummary: DaySummaryV2;
  clinicalServices: ClinicalService[];
};

export type CancelDayFromValues = {
  cancelChoice: 'true' | 'false' | undefined;
};

const CancelDayWizard = ({
  date,
  siteId,
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
        site: siteId,
        date: parsedDate.format(RFC3339Format),
      };

      const response = await fromServer(cancelDay(payload));
      replace(
        `/site/${siteId}/cancel-day/confirmed?date=${date}&cancelledBookingCount=${response.cancelledBookingCount}&bookingsWithoutContactDetails=${response.bookingsWithoutContactDetails}`,
      );
    });
  };

  return (
    <>
      <SessionSummaryTable
        sessionSummaries={daySummary.sessionSummaries}
        clinicalServices={clinicalServices}
        showUnbooked={false}
        tableCaption={`Sessions for ${parsedDate.format('dddd D MMMM')}`}
      />
      <InsetText>
        {daySummary.totalSupportedAppointments +
          daySummary.totalOrphanedAppointments}{' '}
        booked appointments will be cancelled. We'll notify people that their
        appointment has been cancelled
      </InsetText>

      <FormProvider {...methods}>
        <form onSubmit={methods.handleSubmit(submitForm)}></form>
      </FormProvider>
    </>
  );
};

export default CancelDayWizard;
