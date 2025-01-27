import NhsPage from '@components/nhs-page';
import {
  assertPermission,
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
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const [site, wellKnownOdsCodeEntries, sitePermissions] = await Promise.all([
    fetchSite(params.site),
    fetchWellKnownOdsCodeEntries(),
    fetchPermissions(params.site),
    assertPermission(params.site, 'site:view'),
  ]);

  return (
    <NhsPage title={site.name} site={site} originPage="site">
      <SitePage
        site={site}
        permissions={sitePermissions}
        wellKnownOdsCodeEntries={wellKnownOdsCodeEntries}
      />
    </NhsPage>
  );
};

export default Page;
