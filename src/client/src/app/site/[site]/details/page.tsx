import NhsPage from '@components/nhs-page';
import {
  assertAllPermissions,
  fetchPermissions,
  fetchSite,
  fetchWellKnownOdsCodeEntries,
} from '@services/appointmentsService';
import SiteDetailsPage from './site-details-page';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertAllPermissions(siteFromPath, ['site:view', 'site:get-meta-data']);

  const [site, wellKnownOdsCodeEntries, sitePermissions] = await Promise.all([
    fetchSite(siteFromPath),
    fetchWellKnownOdsCodeEntries(),
    fetchPermissions(siteFromPath),
  ]);

  return (
    <NhsPage
      title="Manage Site"
      site={site}
      caption={site.name}
      originPage="site-details"
    >
      <SiteDetailsPage
        siteId={siteFromPath}
        permissions={sitePermissions}
        wellKnownOdsEntries={wellKnownOdsCodeEntries}
      />
    </NhsPage>
  );
};

export default Page;
