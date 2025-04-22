import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchBooking,
  fetchClinicalServices,
  fetchSite,
} from '@services/appointmentsService';
import CancelAppointmentPage from './cancel-appointment-page';
import { notFound } from 'next/navigation';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import {
  dayStringFormat,
  parseDateStringToUkDatetime,
} from '@services/timeService';

type PageProps = {
  params: {
    site: string;
    reference: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertPermission(params.site, 'booking:cancel');

  const [site, booking, clinicalServices] = await Promise.all([
    fetchSite(params.site),
    fetchBooking(params.reference, params.site),
    fetchClinicalServices(),
  ]);

  if (!booking || booking.status === 'Cancelled') {
    notFound();
  }

  const returnDate = parseDateStringToUkDatetime(booking.from).format(
    dayStringFormat,
  );
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
      <CancelAppointmentPage
        booking={booking}
        site={site.id}
        clinicalServices={clinicalServices}
      />
    </NhsPage>
  );
};

export default Page;
