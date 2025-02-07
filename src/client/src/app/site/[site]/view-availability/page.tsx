import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { ViewAvailabilityPage } from './view-availability-page';

type PageProps = {
  searchParams?: Promise<{
    date?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { date } = { ...(await searchParams) };
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'availability:query');
  const site = await fetchSite(siteFromPath);

  const searchMonth = date ? dayjs(date, 'YYYY-MM-DD') : dayjs();

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
