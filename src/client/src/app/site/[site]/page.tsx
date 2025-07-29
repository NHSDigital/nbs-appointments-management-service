import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchFeatureFlag,
  fetchPermissions,
  fetchSite,
  fetchWellKnownOdsCodeEntries,
} from '@services/appointmentsService';
import { SitePage } from './site-page';
import { Metadata } from 'next/types';

// TODO: Get a brief for what titles/description should be on each page
// Could use the generateMetadata function to dynamically generate this, to include site names / other dynamic content
export const metadata: Metadata = {
  title: 'Manage your appointments - Site',
  description: 'Manage appointments at this site',
};

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'site:view');

  const [site, wellKnownOdsCodeEntries, sitePermissions, siteSummaryEnabled] =
    await Promise.all([
      fetchSite(siteFromPath),
      fetchWellKnownOdsCodeEntries(),
      fetchPermissions(siteFromPath),
      fetchFeatureFlag('SiteSummaryReport'),
    ]);

  return (
    <NhsPage title={site.name} site={site} originPage="site">
      <SitePage
        site={site}
        permissions={sitePermissions}
        wellKnownOdsCodeEntries={wellKnownOdsCodeEntries}
        siteSummaryEnabled={siteSummaryEnabled.enabled}
      />
    </NhsPage>
  );
};

export default Page;
