import {
  assertPermission,
  fetchPermissions,
  fetchSite,
  fetchClinicalServices,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { fetchBookings } from '../../../../../lib/services/appointmentsService';
import { FetchBookingsRequest } from '@types';
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
import { Heading } from 'nhsuk-react-components';
import { DayView } from './day-view';

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

  const canCancelBookings = sitePermissions.includes('booking:cancel');

  const canChangeAvailability =
    cancelADateRange.enabled && sitePermissions.includes('availability:setup');

  return (
    <>
      <div className="nhsuk-button-group nhsuk-button-group--small">
        <PrintPageButton />
      </div>

      <Heading headingLevel="h2">
        <span className="nhsuk-caption-l">{site.name}</span>
        {fromDate.format('dddd D MMMM YYYY')}
      </Heading>

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

      <p className="print-out-data" aria-hidden="true">
        Generated: {GetCurrentDateTime()}
      </p>

      <DayView
        fromDate={fromDate}
        bookings={bookings}
        canCancelBookings={canCancelBookings}
        clinicalServices={clinicalServices}
        site={site}
      />
    </>
  );
};

export default Page;
