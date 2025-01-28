import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import { EditInformationForCitizensPage } from './edit-information-for-citizens-page';

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
    <NhsPage title="Site management" originPage="edit-information-for-citizens">
      <h3>Manage information for citizens</h3>
      <EditInformationForCitizensPage
        permissions={sitePermissions}
        site={params.site}
      />
    </NhsPage>
  );
};

export default Page;
