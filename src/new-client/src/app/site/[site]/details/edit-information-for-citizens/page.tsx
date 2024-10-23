import NhsPage from '@components/nhs-page';
import { fetchPermissions, fetchSite } from '@services/appointmentsService';
import { EditInformationForCitizensPage } from './edit-information-for-citizens-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;
  const sitePermissions = await fetchPermissions(params.site);

  if (!sitePermissions.includes('site:manage')) {
    throw new Error('Forbidden: You lack the necessary permissions');
  }

  return (
    <NhsPage
      title="Site management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
      <h3>Manage information for citizens</h3>
      <EditInformationForCitizensPage
        permissions={sitePermissions}
        site={params.site}
      />
    </NhsPage>
  );
};

export default Page;
