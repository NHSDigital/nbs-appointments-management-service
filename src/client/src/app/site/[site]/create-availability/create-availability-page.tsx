import { Button, Spinner } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Site } from '@types';
import { Suspense } from 'react';
import { AvailabilityCreatedEventsTable } from './availabilityCreatedEventsTable';
import { fetchAvailabilityCreatedEvents } from '@services/appointmentsService';

type Props = {
  site: Site;
};

export const CreateAvailabilityPage = async ({ site }: Props) => {
  return (
    <>
      <p>
        You can create availability with multiple days and repeating sessions,
        to accurately reflect your site's capacity.
      </p>
      <br />
      <Suspense fallback={<Spinner />}>
        <AvailabilityCreatedEventsTable
          site={site}
          getAvailabilityCreatedEvents={fetchAvailabilityCreatedEvents(site.id)}
        />
      </Suspense>

      <br />
      <Link href={`/site/${site.id}/create-availability/wizard`}>
        <Button type="button">Create availability</Button>
      </Link>
    </>
  );
};
