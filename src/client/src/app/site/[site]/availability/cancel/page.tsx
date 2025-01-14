import { assertPermission, fetchSite } from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import ConfirmCancellation from './confirm-cancellation';
import NhsPage from '@components/nhs-page';

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
      title="Are you sure you want to cancel this session?"
      caption="Cancel session"
      originPage="edit-session"
    >
      <ConfirmCancellation
        date={searchParams.date}
        session={searchParams.session}
        site={params.site}
      />
    </NhsPage>
  );
};

export default Page;
