import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import YearPage from './year-page';
import { notFound } from 'next/navigation';
import { formatDateForUrl, parseDate } from '@services/timeService';

type PageProps = {
  params: {
    site: string;
  };
  searchParams: {
    date: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const parsedDate = parseDate(searchParams.date);

  const site = await fetchSite(params.site);
  if (!site) {
    notFound();
  }
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  return (
    <NhsPage
      title={`Availability for ${parsedDate.format('YYYY')}`}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        {
          name: 'Availability',
        },
      ]}
    >
      <YearPage referenceDate={formatDateForUrl(parsedDate)} site={site} />
    </NhsPage>
  );
};

export default Page;
