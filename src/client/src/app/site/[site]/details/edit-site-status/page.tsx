import NhsPage from '@components/nhs-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import {
  assertFeatureEnabled,
  assertPermission,
} from '@services/appointmentsService';
import { EditSiteStatusPage } from './edit-site-status-page';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'site:manage');
  await assertFeatureEnabled('SiteStatus');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${siteFromPath}/details`,
    text: 'Back',
  };

  return (
    <NhsPage backLink={backLink} title="" originPage="edit">
      <EditSiteStatusPage siteId={siteFromPath} />
    </NhsPage>
  );
};

export default Page;
