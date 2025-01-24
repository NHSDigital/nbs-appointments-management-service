import NhsPage from '@components/nhs-page';
import { assertPermission } from '@services/appointmentsService';
import { EditDetailsPage } from './edit-details-page';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

export type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertPermission(params.site, 'site:manage');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/details`,
    text: 'Back to site details',
  };

  return (
    <NhsPage backLink={backLink} title="" originPage="edit">
      <EditDetailsPage siteId={params.site} />
    </NhsPage>
  );
};

export default Page;
