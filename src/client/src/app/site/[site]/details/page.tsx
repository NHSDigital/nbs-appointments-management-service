import NhsPage from '@components/nhs-page';
import {
  assertAllPermissions,
  fetchPermissions,
  fetchSite,
} from '@services/appointmentsService';
import SiteDetailsPage from './site-details-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  const sitePermissions = await fetchPermissions(params.site);

  await assertAllPermissions(site.id, ['site:view', 'site:get-meta-data']);

  return (
    <NhsPage
      title="Site details"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
      site={site}
    >
      <SiteDetailsPage site={site} permissions={sitePermissions} />
    </NhsPage>
  );
};

export default Page;
