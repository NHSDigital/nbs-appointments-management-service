import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
} from '@services/appointmentsService';
import EditAccessibilitiesPage from './edit-accessibilities-page';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'site:manage');
  const sitePermissions = await fetchPermissions(siteFromPath);

  return (
    <NhsPage title="Site management" originPage="edit-accessibilities">
      <EditAccessibilitiesPage
        site={siteFromPath}
        permissions={sitePermissions}
      />
    </NhsPage>
  );
};

export default Page;
