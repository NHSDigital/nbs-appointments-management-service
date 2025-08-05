import NhsPage from '@components/nhs-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import { assertPermission } from '@services/appointmentsService';
import { EditSiteStatusPage } from './edit-site-status-page';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  // TODO: Make sure navigation doesn't occur if feature is disabled
  await assertPermission(siteFromPath, 'site:manage');

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
