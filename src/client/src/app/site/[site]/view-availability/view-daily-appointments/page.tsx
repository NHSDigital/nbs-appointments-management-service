import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';

import { ViewDailyAppointmentsPage } from './view-daily-appointments-page';
import dayjs from 'dayjs';

type PageProps = {
  searchParams: {
    date: string;
    page: number;
  };
  params: {
    site: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:query');

  const title = dayjs(searchParams.date).format('dddd D MMMM');

  return (
    <NhsPage
      title={title}
      caption={site.name}
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
        {
          name: 'View daily appointments',
          href: `/site/${params.site}/view-availability/view-daily-appointments?date=${searchParams.date}&page=${searchParams.page}`,
        },
      ]}
    >
      <ViewDailyAppointmentsPage
        page={searchParams.page}
        site={params.site}
        date={searchParams.date}
      />
    </NhsPage>
  );
};

export default Page;
