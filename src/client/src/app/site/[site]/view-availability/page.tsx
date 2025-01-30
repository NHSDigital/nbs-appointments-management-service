import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { ViewAvailabilityPage } from './view-availability-page';

type PageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    date?: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  await assertPermission(params.site, 'availability:query');
  const site = await fetchSite(params.site);

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
      <ViewAvailabilityPage site={site} searchMonth={searchMonth} />
    </NhsPage>
  );
};

export default Page;
