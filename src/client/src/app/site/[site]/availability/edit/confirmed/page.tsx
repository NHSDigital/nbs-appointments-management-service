import { assertPermission, fetchSite } from '@services/appointmentsService';
import { clinicalServices, SessionSummary } from '@types';
import NhsPage from '@components/nhs-page';
import dayjs from 'dayjs';
import { InsetText, Table } from '@components/nhsuk-frontend';
import Link from 'next/link';

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
  const date = dayjs(searchParams.date, 'YYYY-MM-DD');

  const sessionSummary: SessionSummary = JSON.parse(atob(searchParams.session));

  return (
    <NhsPage
      originPage="edit-session"
      title={`Edit time and capacity for ${date.format('DD MMMM YYYY')}`}
      caption={site.name}
      backLink={{
        href: `/site/${site.id}/view-availability/week/?date=${searchParams.date}`,
        renderingStrategy: 'server',
        text: 'Back to week view',
      }}
    >
      <Table
        headers={['Time', 'Services']}
        rows={[
          [
            <strong
              key={`session-0-start-and-end-time`}
            >{`${dayjs(sessionSummary.start).format('HH:mm')} - ${dayjs(sessionSummary.end).format('HH:mm')}`}</strong>,
            Object.keys(sessionSummary.bookings).map(
              (service, serviceIndex) => {
                return (
                  <span key={`session-0-service-name-${serviceIndex}`}>
                    {clinicalServices.find(cs => cs.value === service)?.label}
                    <br />
                  </span>
                );
              },
            ),
          ],
        ]}
      />
      <InsetText>
        <p>
          Some booked appointments may be affected by this change. If so, you'll
          need to cancel these appointments manually.
        </p>
        <Link
          href={`/site/${site.id}/view-availability/daily-appointments?date=${searchParams.date}&page=1&tab=2`}
        >
          Cancel appointments
        </Link>
      </InsetText>
    </NhsPage>
  );
};

export default Page;
