import NhsPage from '@components/nhs-page';
import fromServer from '@server/fromServer';
import { fetchSite } from '@services/appointmentsService';
import AvailabilitySecondaryNav from './availability-secondary-nav';

type LayoutProps = {
  children: React.ReactNode;
  params: {
    site: string;
  };
};

export default async function Layout({ children, params }: LayoutProps) {
  const site = await fromServer(fetchSite(params.site));

  return (
    <NhsPage
      title="Test title"
      site={site}
      originPage="view-availability"
      secondaryNavigation={<AvailabilitySecondaryNav site={params.site} />}
    >
      {children}
    </NhsPage>
  );
}
