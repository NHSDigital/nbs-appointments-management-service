import { Table, InsetText } from '@components/nhsuk-frontend';
import {
  SessionServicesCell,
  SessionTimesCell,
} from '@components/session-summary-table';
import { SessionSummary, Site } from '@types';
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
            <SessionTimesCell
              key={`session-0-start-and-end-time`}
              sessionSummary={sessionSummary}
            />,
            <SessionServicesCell
              key={`session-0-service-name`}
              sessionSummary={sessionSummary}
            />,
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
