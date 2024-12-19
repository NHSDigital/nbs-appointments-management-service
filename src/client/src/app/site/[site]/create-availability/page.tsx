import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  await assertPermission(site.id, 'availability:set-setup');

  return (
    <NhsPage title="Create availability" caption={site.name} site={site}>
      <CreateAvailabilityPage site={site} />
    </NhsPage>
  );
};

export default Page;
