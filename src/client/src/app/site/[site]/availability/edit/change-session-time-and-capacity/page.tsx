import { assertPermission, fetchSite } from '@services/appointmentsService';
import ChangeSessionTimeAndCapacityPage from './change-session-time-and-capacity';
import { notFound } from 'next/navigation';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import { parseToUkDatetime } from '@services/timeService';
import fromServer from '@server/fromServer';

type PageProps = {
  params: {
    site: string;
  };
  searchParams: {
    date?: string;
    orphanedCount?: string;
    bookingCount?: string;
    bookings?: string;
    updatedSession?: string;
    originalSession?: string;
  };
};

const Page = async ({ params, searchParams }: PageProps) => {
  const resolvedParams = await params;
  const resolvedSearchParams = await searchParams;

  const siteFromPath = resolvedParams.site;
  const { date, orphanedCount, bookingCount, updatedSession, originalSession } =
    resolvedSearchParams;

  await fromServer(assertPermission(siteFromPath, 'availability:setup'));

  const parsedDate = parseToUkDatetime(date?.toString() || '');
  const [site] = await Promise.all([fromServer(fetchSite(siteFromPath))]);

  if (
    !updatedSession ||
    !originalSession ||
    !date ||
    orphanedCount === undefined ||
    bookingCount === undefined
  ) {
    notFound();
  }

  let decodedUpdated, decodedOriginal, decodedBookings;
  try {
    decodedUpdated = JSON.parse(atob(decodeURIComponent(updatedSession)));
    decodedOriginal = JSON.parse(atob(decodeURIComponent(originalSession)));
    decodedBookings = JSON.parse(
      atob(decodeURIComponent(searchParams.bookings || '[]')),
    );
  } catch (e) {
    notFound();
  }

  return (
    <NhsTransactionalPage
      title={`New time and capacity for ${parsedDate.format('DD MMMM YYYY')}`}
      caption={site.name}
      originPage="bulk-appointment-cancel"
      backLink={{
        href: `/site/${site.id}/view-availability/week/edit-session?session=${originalSession}&date=${date}`,
        renderingStrategy: 'server',
        text: 'Go back',
      }}
    >
      <ChangeSessionTimeAndCapacityPage
        site={site}
        orphanedCount={Number(orphanedCount)}
        bookingsCount={Number(bookingCount)}
        updatedSession={decodedUpdated}
        originalSession={decodedOriginal}
        bookingReferences={decodedBookings}
        date={date}
      />
    </NhsTransactionalPage>
  );
};

export default Page;
