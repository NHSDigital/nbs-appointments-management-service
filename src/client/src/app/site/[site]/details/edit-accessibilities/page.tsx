import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import { EditAccessibilitiesPage } from './edit-accessibilities-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const sitePermissions = await fetchPermissions(params.site);

  await assertPermission(site.id, 'site:manage');

  return (
    <NhsPage
      title="Site management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
      originPage="edit-accessibilities"
    >
      <EditAccessibilitiesPage
        site={params.site}
        permissions={sitePermissions}
      />
    </NhsPage>
  );
};

export default Page;
