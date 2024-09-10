import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import SessionPage from './session-page';

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
      title="Manage sessions for this day"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        {
          name: 'Availability Overview',
          href: `/site/${params.site}/availability`,
        },
        {
          name: 'Week',
          href: `/site/${params.site}/availability/week`,
        },
      ]}
    >
      <SessionPage />
    </NhsPage>
  );
};

export default Page;
