import NhsPage from '@components/nhs-page';
import { fetchSite } from '@services/appointmentsService';
import { ManageAttributesPage } from './manage-attributes-page';

export type PageProps = {
  params: {
    site: string;
  };
};
const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  return (
    <NhsPage
      title="Site management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
      <ManageAttributesPage site={params.site} />
    </NhsPage>
  );
};

export default Page;
