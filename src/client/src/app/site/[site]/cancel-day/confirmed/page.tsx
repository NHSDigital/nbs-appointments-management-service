import { assertPermission, fetchSite } from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './confirm-day-cancellation';
import { parseToUkDatetime } from '@services/timeService';
import { CancelDayResponse } from '@types';
import NhsTransactionalPage from '@components/nhs-transactional-page';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    cancelledBookingCount: number;
    bookingsWithoutContactDetails: number;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, cancelledBookingCount, bookingsWithoutContactDetails } = {
    ...(await searchParams),
  };

  await assertPermission(siteFromPath, 'availability:setup');
  if (date === undefined) {
    notFound();
  }

  const site = await fetchSite(siteFromPath);

  const dayCancellationSummary: CancelDayResponse = {
    cancelledBookingCount: cancelledBookingCount ?? 0,
    bookingsWithoutContactDetails: bookingsWithoutContactDetails ?? 0,
  };

  return (
    <NhsTransactionalPage
      title={`${parseToUkDatetime(date).format('dddd D MMMM')} cancelled`}
      caption={`${site.name}`}
      originPage="edit-session"
    >
      <CancellationConfirmed
        date={date}
        site={site.id}
        dayCancellationSummary={dayCancellationSummary}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
