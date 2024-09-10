import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import AvailabilityOverviewPage from './availability-overview-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  return (
    <NhsPage
      title="Availability Overview"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
      <AvailabilityOverviewPage />
    </NhsPage>
  );
};

export default Page;
