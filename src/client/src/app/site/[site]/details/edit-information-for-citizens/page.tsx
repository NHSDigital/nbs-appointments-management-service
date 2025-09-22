import {
  assertPermission,
  fetchPermissions,
} from '@services/appointmentsService';
import { EditInformationForCitizensPage } from './edit-information-for-citizens-page';
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
      title="Site management"
      originPage="edit-information-for-citizens"
    >
      <h3>Manage information for citizens</h3>
      <EditInformationForCitizensPage
        permissions={sitePermissions}
        site={siteFromPath}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
