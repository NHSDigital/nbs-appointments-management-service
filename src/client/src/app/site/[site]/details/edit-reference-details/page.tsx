import { assertPermission } from '@services/appointmentsService';
import { EditReferenceDetailsPage } from './edit-reference-details-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

export type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(assertPermission(siteFromPath, 'site:manage:admin'));

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${siteFromPath}/details`,
    text: 'Go back',
  };

  return (
    <NhsTransactionalPage backLink={backLink} title="" originPage="edit">
      <EditReferenceDetailsPage siteId={siteFromPath} />
    </NhsTransactionalPage>
  );
};

export default Page;
