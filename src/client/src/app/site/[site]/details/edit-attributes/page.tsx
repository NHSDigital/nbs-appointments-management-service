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
  const [site, sitePermissions] = await Promise.all([
    fetchSite(params.site),
    fetchPermissions(params.site),
    assertPermission(params.site, 'site:manage'),
  ]);

  return (
    <NhsPage title="Site management" originPage="edit-attributes">
      <EditAttributesPage site={params.site} permissions={sitePermissions} />
    </NhsPage>
  );
};

export default Page;
