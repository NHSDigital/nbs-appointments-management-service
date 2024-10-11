import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  return (
    <NhsPage
      title="Availability periods"
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      <CreateAvailabilityPage site={site} />
    </NhsPage>
  );
};

export default Page;
