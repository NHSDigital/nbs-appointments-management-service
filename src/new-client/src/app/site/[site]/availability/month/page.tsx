import { fetchSite } from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import MonthPage from './month-page';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import { parsedEnglishDateStringOrToday } from '@services/timeService';

type PageProps = {
  params: {
    site: string;
  };
  searchParams: {
    date: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);
  if (!site) {
    notFound();
  }

  const siteMoniker = site?.name ?? `Site ${params.site}`;

  const parsedDate =
    parsedEnglishDateStringOrToday(searchParams.date) ?? dayjs();
  const monthName = parsedDate.format('MMMM YYYY');

  return (
    <NhsPage
      title={`Availability for ${monthName}`}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        {
          name: 'Availability Overview',
          href: `/site/${params.site}/availability`,
        },
      ]}
    >
      <MonthPage referenceDate={parsedDate} site={site} />
    </NhsPage>
  );
};

export default Page;
