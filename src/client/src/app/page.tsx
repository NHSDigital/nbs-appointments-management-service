import { fetchSitesPreview } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { Metadata } from 'next';
import { HomePage } from './home-page';

export const metadata: Metadata = {
  title: 'Manage your appointments',
  description: 'A National Booking Service site for managing NHS appointments',
};

const Page = async () => {
  const sites = await fetchSitesPreview();

  return (
    <NhsPage
      title="Manage your appointments"
      omitTitleFromBreadcrumbs
      originPage="choose-site"
    >
      <HomePage sites={sites} />
    </NhsPage>
  );
};

export default Page;
