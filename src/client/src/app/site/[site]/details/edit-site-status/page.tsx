import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import {
  assertFeatureEnabled,
  assertPermission,
} from '@services/appointmentsService';
import { EditSiteStatusPage } from './edit-site-status-page';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'site:manage'));
  await fromServer(assertFeatureEnabled('SiteStatus'));

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${siteFromPath}/details`,
    text: 'Back',
  };

  return (
    <NhsTransactionalPage backLink={backLink} title="" originPage="edit">
      <EditSiteStatusPage siteId={siteFromPath} />
    </NhsTransactionalPage>
  );
};

export default Page;
