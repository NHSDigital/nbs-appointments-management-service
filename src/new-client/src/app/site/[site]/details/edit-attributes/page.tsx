import NhsPage from '@components/nhs-page';
import { fetchPermissions, fetchSite } from '@services/appointmentsService';
import { SiteAttributesPage } from './edit-attributes-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;
  const sitePermissions = await fetchPermissions(params.site);

  if (sitePermissions.includes('site:manage') === false) {
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
      <SiteAttributesPage site={params.site} permissions={sitePermissions} />
    </NhsPage>
  );
};

export default Page;
