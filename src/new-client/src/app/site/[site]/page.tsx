import NhsPage from '@components/nhs-page';
import { fetchPermissions, fetchSite } from '@services/appointmentsService';
import { SitePage } from './site-page';
import { Metadata } from 'next/types';

// TODO: Get a brief for what titles/description should be on each page
// Could use the generateMetadata function to dynamically generate this, to include site names / other dynamic content
export const metadata: Metadata = {
  title: 'Appointment Management Service - Site',
  description: 'Manage appointments at this site',
};

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const sitePermissions = await fetchPermissions(params.site);

  // TODO: Because we rely on fetchUserProfile() to get the site,
  // we can't differentiate between being logged out and the site not being found.
  // NhsPage automatically handles the case of the user being logged out, but
  // before it renders we need to know what to pass as title and breadcrumbs
  const siteMoniker = site?.name ?? `Site ${params.site}`;
  return (
    <NhsPage breadcrumbs={[{ name: 'Home', href: '/' }]} title={siteMoniker}>
      <SitePage site={site} permissions={sitePermissions} />
    </NhsPage>
  );
};

export default Page;
