import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchBooking,
  fetchSite,
} from '@services/appointmentsService';
import { CancelAppointmentPage } from './cancel-appointment-page';

type PageProps = {
  params: {
    site: string;
    reference: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'booking:cancel');

  const booking = await fetchBooking(params.reference);

  return (
    <NhsPage
      caption="Cancel appointment"
      title="Are you sure you want to cancel this appointment?"
      breadcrumbs={[{ name: 'Home', href: '/' }]}
      site={site}
    >
      <CancelAppointmentPage booking={booking} />
    </NhsPage>
  );
};

export default Page;
