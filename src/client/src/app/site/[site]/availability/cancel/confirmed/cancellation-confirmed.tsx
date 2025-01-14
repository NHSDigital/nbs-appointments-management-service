import { Table, InsetText } from '@components/nhsuk-frontend';
import { SessionSummary, clinicalServices } from '@types';
import dayjs from 'dayjs';
import Link from 'next/link';

type PageProps = {
  session: string;
  site: string;
  date: string;
};

const CancellationConfirmed = ({ session, site, date }: PageProps) => {
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
  return (
    <>
      <Table
        headers={['Time', 'Services']}
        rows={[
          [
            <strong
              key={0}
            >{`${dayjs(sessionSummary.start).format('HH:mm')} - ${dayjs(sessionSummary.end).format('HH:mm')}`}</strong>,
            Object.keys(sessionSummary.bookings).map((service, k) => {
              return (
                <span key={k}>
                  {clinicalServices.find(cs => cs.value === service)?.label}
                  <br />
                </span>
              );
            }),
          ],
        ]}
      />

      <InsetText>
        <p>You'll need to manually cancel any affected appointments.</p>
        <Link
          href={`/site/${site}/view-availability/daily-appointments?date=${date}&page=1&tab=2`}
        >
          Cancel appointments
        </Link>
      </InsetText>
    </>
  );
};

export default CancellationConfirmed;
