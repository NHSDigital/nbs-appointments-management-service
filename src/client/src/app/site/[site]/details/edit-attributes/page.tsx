import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import { EditAttributesPage } from './edit-attributes-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertPermission(params.site, 'site:manage');
  const [site, sitePermissions] = await Promise.all([
    fetchSite(params.site),
    fetchPermissions(params.site),
  ]);

  return (
    <NhsPage title="Site management" originPage="edit-attributes">
      <EditAttributesPage site={params.site} permissions={sitePermissions} />
    </NhsPage>
  );
};

export default Page;
