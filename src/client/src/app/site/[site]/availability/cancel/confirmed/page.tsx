import NhsPage from '@components/nhs-page';
import { assertPermission, fetchSite } from '@services/appointmentsService';
import dayjs from 'dayjs';
import { notFound } from 'next/navigation';
import CancellationConfirmed from './cancellation-confirmed';

type PageProps = {
  searchParams: {
    date: string;
    session: string;
  };
  params: {
    site: string;
  };
};

const Page = async ({ searchParams, params }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'availability:setup');

  if (searchParams.session === undefined || searchParams.date === undefined) {
    notFound();
  }

  return (
    <NhsPage
      title={`Cancelled session for ${dayjs(searchParams.date).format('DD MMMM YYYY')}`}
      caption={`${site.name}`}
      originPage="edit-session"
    >
      <CancellationConfirmed
        session={searchParams.session}
        date={searchParams.date}
        site={params.site}
      />
    </NhsPage>
  );
};

export default Page;
