import { Table, InsetText } from '@components/nhsuk-frontend';
import {
  SessionServicesCell,
  SessionTimesCell,
} from '@components/session-summary-table';
import { ClinicalService, SessionSummary } from '@types';
import Link from 'next/link';

type PageProps = {
  session: string;
  site: string;
  date: string;
  clinicalServices: ClinicalService[];
};

const CancellationConfirmed = ({
  session,
  site,
  date,
  clinicalServices,
}: PageProps) => {
  const sessionSummary: SessionSummary = JSON.parse(atob(session));
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
              clinicalServices={clinicalServices}
            />,
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
