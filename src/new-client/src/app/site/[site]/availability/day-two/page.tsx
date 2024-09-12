import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { notFound } from 'next/navigation';
import { formatDateForUrl, parseDate } from '@services/timeService';
import DayPage from './day-page';

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
      title={`${parsedDate.format('DD MMMM YYYY')} - ${parsedDate.format('dddd')}`}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        {
          name: 'Availability',
        },
        {
          name: 'Year',
          href: `/site/${params.site}/availability/year?date=${formatDateForUrl(parsedDate)}`,
        },
        {
          name: 'Month',
          href: `/site/${params.site}/availability/month?date=${formatDateForUrl(parsedDate)}`,
        },
        {
          name: 'Week',
          href: `/site/${params.site}/availability/week?date=${formatDateForUrl(parsedDate)}`,
        },
      ]}
    >
      <DayPage referenceDate={formatDateForUrl(parsedDate)} site={site} />
    </NhsPage>
  );
};

export default Page;
