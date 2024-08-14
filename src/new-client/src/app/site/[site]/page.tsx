import NhsPage from '@components/nhs-page';
import { fetchSite } from '@services/appointmentsService';
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

  return (
    <NhsPage title={site.name} breadcrumbs={[{ name: site.name }]}>
      <SitePage site={site} />
    </NhsPage>
  );
};

export default Page;
