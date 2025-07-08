import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const props = await params;
  await assertPermission(props.site, 'availability:setup');
  const site = await fetchSite(props.site);

  return (
    <NhsPage
      title="Create availability"
      caption={site.name}
      site={site}
      originPage="create-availability"
    >
      <CreateAvailabilityPage site={site} />
    </NhsPage>
  );
};

export default Page;
