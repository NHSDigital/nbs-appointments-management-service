import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { ViewAvailabilityPage } from './view-availability-page';
import { Suspense } from 'react';
import { Spinner } from '@components/nhsuk-frontend';

type PageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    date?: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');
  const searchMonth = searchParams?.date
    ? dayjs(searchParams?.date, 'YYYY-MM-DD')
    : dayjs();

  return (
    <NhsPage
      title={`View availability for ${searchMonth.format('MMMM YYYY')}`}
      caption={site.name}
      site={site}
      originPage="view-availability"
    >
      <Suspense fallback={<Spinner />}>
        <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
      </Suspense>
    </NhsPage>
  );
};

export default Page;
