import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  return (
    <NhsPage
      title="Create Availability"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
      {/* This will be covered in APPT-240 */}
      <span></span>
    </NhsPage>
  );
};

export default Page;
