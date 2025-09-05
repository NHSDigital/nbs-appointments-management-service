import { assertPermission } from '@services/appointmentsService';
import { EditDetailsPage } from './edit-details-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import NhsTransactionalPage from '@components/nhs-transactional-page';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertPermission(siteFromPath, 'site:manage');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${siteFromPath}/details`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage backLink={backLink} title="" originPage="edit">
      <EditDetailsPage siteId={siteFromPath} />
    </NhsTransactionalPage>
  );
};

export default Page;
