import NhsPage from '@components/nhs-page';
import { assertPermission } from '@services/appointmentsService';
import { EditDetailsPage } from './edit-details-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

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
    <NhsPage backLink={backLink} title="" originPage="edit">
      <EditDetailsPage siteId={siteFromPath} />
    </NhsPage>
  );
};

export default Page;
