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
  params: Promise<{
    site: string;
    reference: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath, reference } = { ...(await params) };

  await assertPermission(siteFromPath, 'booking:cancel');

  const [site, booking] = await Promise.all([
    fetchSite(siteFromPath),
    fetchBooking(reference, siteFromPath),
  ]);

  if (!booking || booking.status === 'Cancelled') {
    notFound();
  }

  const returnDate = dayjs(booking.from).format('YYYY-MM-DD');
  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/daily-appointments?date=${returnDate}&page=1`,
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
