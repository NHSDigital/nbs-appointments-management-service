import { Table, InsetText } from '@components/nhsuk-frontend';
import { SessionSummary, Site, clinicalServices } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type PageProps = {
  sessionSummary: SessionSummary;
  site: Site;
  date: string;
};

const EditSessionConfirmed = ({ sessionSummary, site, date }: PageProps) => {
  return (
    <>
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
          href={`/site/${site.id}/view-availability/daily-appointments?date=${date}&page=1&tab=2`}
        >
          Cancel appointments
        </Link>
      </InsetText>
    </>
  );
};

export default EditSessionConfirmed;
