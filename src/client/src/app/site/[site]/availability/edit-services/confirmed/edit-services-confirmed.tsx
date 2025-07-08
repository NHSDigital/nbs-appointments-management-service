import { Table, InsetText } from '@components/nhsuk-frontend';
import { AvailabilitySession, ClinicalService, Site } from '@types';
import Link from 'next/link';

type PageProps = {
  removedServicesSession: Promise<AvailabilitySession>;
  clinicalServices: Promise<ClinicalService[]>;
  site: Promise<Site>;
  date: Promise<string>;
};

const EditServicesConfirmed = async ({
  removedServicesSession,
  clinicalServices,
  site,
  date,
}: PageProps) => {
  const [removed, services, siteData, dateData] = await Promise.all([
    removedServicesSession,
    clinicalServices,
    site,
    date,
  ]);

  return (
    <>
      <Table
        headers={['Time', 'Services']}
        rows={[
          [
            <strong key={`session-0-start-and-end-time`}>
              {`${removed.from} - ${removed.until}`}
            </strong>,
            <>
              {removed.services.map((service, serviceIndex) => {
                return (
                  <span key={`service-name-${serviceIndex}`}>
                    {services.find(c => c.value === service)?.label ?? service}
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
          href={`/site/${siteData.id}/view-availability/daily-appointments?date=${dateData}&page=1&tab=2`}
        >
          Cancel appointments
        </Link>
      </InsetText>
    </>
  );
};

export default EditServicesConfirmed;
