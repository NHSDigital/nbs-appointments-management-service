import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchBooking,
  fetchSite,
} from '@services/appointmentsService';
import CancelAppointmentPage from './cancel-appointment-page';
import { notFound } from 'next/navigation';
import dayjs from 'dayjs';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';

type PageProps = {
  params: {
    site: string;
    reference: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  await assertPermission(site.id, 'booking:cancel');

  const booking = await fetchBooking(params.reference, site.id);
  if (!booking || booking.status === 'Cancelled') {
    notFound();
  }

  const returnDate = dayjs(booking.from).format('YYYY-MM-DD');
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${params.site}/view-availability/daily-appointments?date=${returnDate}&page=1`,
    text: 'Go back',
  };

  return (
    <NhsPage
      caption="Cancel appointment"
      title="Are you sure you want to cancel this appointment?"
      originPage="appointment-cancel"
      backLink={backLink}
      site={site}
    >
      <CancelAppointmentPage booking={booking} site={site.id} />
    </NhsPage>
  );
};

export default Page;
