import {
  assertPermission,
  fetchPermissions,
} from '@services/appointmentsService';
import EditAccessibilitiesPage from './edit-accessibilities-page';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'site:manage'));
  const sitePermissions = await fromServer(fetchPermissions(siteFromPath));

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
