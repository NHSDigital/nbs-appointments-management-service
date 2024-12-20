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
  const site = await fetchSite(params.site);
  const wellKnownOdsCodeEntries = await fetchWellKnownOdsCodeEntries();

  const sitePermissions = await fetchPermissions(params.site);

  await assertAllPermissions(site.id, [
    'site:get-config',
    'site:get-meta-data',
  ]);

  return (
    <NhsPage title="Manage Site" site={site} caption={site.name}>
      <SiteDetailsPage
        site={site}
        permissions={sitePermissions}
        wellKnownOdsEntries={wellKnownOdsCodeEntries}
      />
    </NhsPage>
  );
};

export default Page;
