import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import MonthPage from './month-page';

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
      title="Week Availability"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        {
          name: 'Availability Overview',
          href: `/site/${params.site}/availability`,
        },
      ]}
    >
      <MonthPage />
    </NhsPage>
  );
};

export default Page;
