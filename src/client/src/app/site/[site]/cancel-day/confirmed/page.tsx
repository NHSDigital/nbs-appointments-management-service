import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchDayCancellationSummary,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './confirm-day-cancellation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { parseToUkDatetime } from '@services/timeService';

type PageProps = {
  searchParams?: Promise<{
    date: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ searchParams, params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date } = { ...(await searchParams) };

  await assertPermission(siteFromPath, 'availability:setup');
  if (date === undefined) {
    notFound();
  }

  const [site, dayCancellationSummary] = await Promise.all([
    fetchSite(siteFromPath),
    fetchDayCancellationSummary(siteFromPath, date),
  ]);

  return (
    <NhsPage
      title={`${parseToUkDatetime(date).format('dddd DD MMMM')} cancelled`}
      caption={`${site.name}`}
      originPage="edit-session"
    >
      <CancellationConfirmed
        date={date}
        site={site.id}
        dayCancellationSummary={dayCancellationSummary}
      />
    </NhsPage>
  );
};

export default Page;
