import NhsPage from '@components/nhs-page';
import {
  assertAllPermissions,
  fetchPermissions,
  fetchSite,
  fetchWellKnownOdsCodeEntries,
} from '@services/appointmentsService';
import SiteDetailsPage from './site-details-page';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const [site, wellKnownOdsCodeEntries, sitePermissions] = await Promise.all([
    fetchSite(params.site),
    fetchWellKnownOdsCodeEntries(),
    fetchPermissions(params.site),
    assertAllPermissions(params.site, ['site:view', 'site:get-meta-data']),
  ]);

  return (
    <NhsPage
      title="Manage Site"
      site={site}
      caption={site.name}
      originPage="site-details"
    >
      <SiteDetailsPage
        site={site}
        permissions={sitePermissions}
        wellKnownOdsEntries={wellKnownOdsCodeEntries}
      />
    </NhsPage>
  );
};

export default Page;
