import NhsPage from '@components/nhs-page';
import { assertPermission } from '@services/appointmentsService';
import { EditReferenceDetailsPage } from './edit-reference-details-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertPermission(params.site, 'site:manage:admin');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/details`,
    text: 'Go back',
  };

  return (
    <NhsPage backLink={backLink} title="" originPage="edit">
      <EditReferenceDetailsPage siteId={params.site} />
    </NhsPage>
  );
};

export default Page;
