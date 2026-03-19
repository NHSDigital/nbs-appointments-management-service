import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchPermissions,
  fetchSite,
  fetchClinicalServices,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { fetchBookings } from '../../../../lib/services/appointmentsService';
import { DailyAppointmentsPage } from './daily-appointments-page';
import { FetchBookingsRequest } from '@types';
import { Tab, Tabs } from '@nhsuk-frontend-components';
import { NavigationByHrefProps } from '@components/nhsuk-frontend/back-link';
import {
  dateTimeFormat,
  parseToUkDatetime,
  GetCurrentDateTime,
} from '@services/timeService';
import { notFound } from 'next/navigation';
import fromServer from '@server/fromServer';
import { Button } from '@components/nhsuk-frontend';
import PrintPageButton from '@components/print-page-button';
import Link from 'next/link';

type PageProps = {
  searchParams?: Promise<{
    date: string;
    page: number;
    tab?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { date, page } = { ...(await searchParams) };
  if (date === undefined || page === undefined) {
    return notFound();
  }

  await fromServer(assertPermission(siteFromPath, 'booking:view-detail'));

  const fromDate = parseToUkDatetime(date);
  const toDate = fromDate.endOf('day');

  const fetchBookingsRequest: FetchBookingsRequest = {
    from: fromDate.format(dateTimeFormat),
    to: toDate.format(dateTimeFormat),
    site: siteFromPath,
  };

  const [site, bookings, clinicalServices, cancelADateRange, sitePermissions] =
    await Promise.all([
      fromServer(fetchSite(siteFromPath)),
      fromServer(fetchBookings(fetchBookingsRequest, ['Booked', 'Cancelled'])),
      fromServer(fetchClinicalServices()),
      fromServer(fetchFeatureFlag('CancelADateRange')),
      fromServer(fetchPermissions(siteFromPath)),
    ]);

  const bookedAppointments = bookings.filter(b => b.status === 'Booked');
  const cancelledAppointments = bookings.filter(b => b.status === 'Cancelled');

  const backLink: NavigationByHrefProps = {
    renderingStrategy: 'server',
    href: `/site/${site.id}/view-availability/week?date=${date}`,
    text: 'Back to week view',
  };

  const canCancelBookings = sitePermissions.includes('booking:cancel');

  const canChangeAvailability =
    cancelADateRange.enabled && sitePermissions.includes('availability:setup');

  return (
    <NhsPage
      title={fromDate.format('dddd D MMMM')}
      caption={site.name}
      backLink={backLink}
      originPage="view-availability-daily-appointments"
      site={site}
    >
      {canChangeAvailability && (
        <Link
          href={`/site/${siteFromPath}/change-availability`}
          className="no-print nhsuk-u-margin-right-3"
        >
          <Button type="button" styleType="secondary">
            Change availability
          </Button>
        </Link>
      )}

      <span>
        <PrintPageButton />
      </span>

      <p className="print-out-data" aria-hidden="true">
        Generated: {GetCurrentDateTime()}
      </p>

      <Tabs paramsToSetOnTabChange={[{ key: 'page', value: '1' }]}>
        <Tab title="Scheduled">
          <div className="print-out-data" aria-hidden="true">
            <h3>Scheduled Appointments</h3>
          </div>
          <DailyAppointmentsPage
            bookings={bookedAppointments}
            site={site.id}
            displayAction={canCancelBookings}
            clinicalServices={clinicalServices}
          />
        </Tab>
        <Tab title="Cancelled">
          <div className="print-out-data" aria-hidden="true">
            <h3>Cancelled Appointments</h3>
          </div>
          <DailyAppointmentsPage
            bookings={cancelledAppointments}
            site={site.id}
            displayAction={false}
            clinicalServices={clinicalServices}
          />
        </Tab>
      </Tabs>
    </NhsPage>
  );
};

export default Page;
