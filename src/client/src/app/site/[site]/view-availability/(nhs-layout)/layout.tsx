import NhsPage from '@components/nhs-page';
import fromServer from '@server/fromServer';
import { fetchSite } from '@services/appointmentsService';
import { AvailabilitySecondaryNavigation } from './availability-secondary-navigation';

type LayoutProps = {
  children: React.ReactNode;
  params: Promise<{
    site: string;
  }>;
};

export default async function Layout({ children, params }: LayoutProps) {
  const { site: siteFromPath } = { ...(await params) };
  const site = await fromServer(fetchSite(siteFromPath));

  return (
    <NhsPage
      site={site}
      originPage="view-availability"
      secondaryNavigation={<AvailabilitySecondaryNavigation site={site.id} />}
    >
      {children}
    </NhsPage>
  );
}
