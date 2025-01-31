import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
} from '@services/appointmentsService';
import { EditAccessibilitiesPage } from './edit-accessibilities-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertPermission(params.site, 'site:manage');
  const sitePermissions = await fetchPermissions(params.site);

  return (
    <NhsPage title="Site management" originPage="edit-accessibilities">
      <EditAccessibilitiesPage
        site={params.site}
        permissions={sitePermissions}
      />
    </NhsPage>
  );
};

export default Page;
