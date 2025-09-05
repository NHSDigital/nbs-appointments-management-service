import {
  assertPermission,
  fetchPermissions,
} from '@services/appointmentsService';
import EditAccessibilitiesPage from './edit-accessibilities-page';
import NhsTransactionalPage from '@components/nhs-transactional-page';

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
    <NhsTransactionalPage
      title="Edit accessibilities"
      originPage="edit-accessibilities"
    >
      <EditAccessibilitiesPage
        site={siteFromPath}
        permissions={sitePermissions}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
