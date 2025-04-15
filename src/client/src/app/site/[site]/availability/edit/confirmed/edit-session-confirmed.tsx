import { Table, InsetText } from '@components/nhsuk-frontend';
import { AvailabilitySession, ClinicalService, Site } from '@types';
import Link from 'next/link';

type PageProps = {
  updatedSession: AvailabilitySession;
  clinicalServices: ClinicalService[];
  site: Site;
  date: string;
};

const EditSessionConfirmed = ({
  updatedSession,
  clinicalServices,
  site,
  date,
}: PageProps) => {
  return (
    <>
      <Table
        headers={['Time', 'Services']}
        rows={[
          [
            <strong key={`session-0-start-and-end-time`}>
              {`${updatedSession.from} - ${updatedSession.until}`}
            </strong>,
            <>
              {updatedSession.services.map((service, serviceIndex) => {
                return (
                  <span key={`service-name-${serviceIndex}`}>
                    {clinicalServices.find(c => c.value === service)?.label ??
                      service}
                    <br />
                  </span>
                );
              })}
            </>,
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
