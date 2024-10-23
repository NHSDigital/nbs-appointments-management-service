import NhsPage from '@components/nhs-page';
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
    <NhsPage
      title="Create Availability"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
    >
      {/* This will be covered in APPT-240 */}
      <span></span>
    </NhsPage>
  );
};

export default Page;
