import { fetchSitesPreview } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { Metadata } from 'next';
import { SitesPage } from './sites-page';
import fromServer from '@server/fromServer';

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async () => {
  const sites = await fromServer(fetchSitesPreview());

  return (
    <NhsPage
      title="Viewing all sites"
      omitTitleFromBreadcrumbs
      originPage="choose-site"
    >
      <SitesPage sites={sites} />
    </NhsPage>
  );
};

export default Page;
