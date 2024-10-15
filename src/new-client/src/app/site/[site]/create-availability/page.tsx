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
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  if (site === undefined) {
    throw new Error('Site not found');
  }

  return (
    <NhsPage
      title="Availability periods"
      caption={siteMoniker}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
      <CreateAvailabilityPage site={site} />
    </NhsPage>
  );
};

export default Page;
