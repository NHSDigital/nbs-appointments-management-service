import NhsPage from '@components/nhs-page';
import { CreateAvailabilityPage } from './create-availability-page';
import {
  assertPermission,
  fetchAvailabilityCreatedEvents,
  fetchSite,
} from '@services/appointmentsService';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  await assertPermission(site.id, 'availability:set-setup');

  const availabilityCreated = await fetchAvailabilityCreatedEvents(site.id);

  return (
    <NhsPage
      title="Create availability"
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      <CreateAvailabilityPage
        site={site}
        availabilityCreated={availabilityCreated}
      />
    </NhsPage>
  );
};

export default Page;
