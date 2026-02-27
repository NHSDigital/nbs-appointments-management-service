import { Button, ButtonGroup, Spinner } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Suspense } from 'react';
import { AvailabilityCreatedEventsTable } from './availabilityCreatedEventsTable';
import { Site } from '@types';

type Props = {
  site: Site;
};

export const CreateAvailabilityPage = ({ site }: Props) => {
  return (
    <>
      <ButtonGroup>
        <Link href={`/site/${site.id}/create-availability/wizard`}>
          <Button type="button">Create new availability</Button>
        </Link>
        <Link href={`/site/${site.id}/change-availability`}>
          <Button type="button" styleType="secondary">
            Change availability
          </Button>
        </Link>
      </ButtonGroup>
      <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
        History of sessions you created
      </legend>
      <p>
        If you cancel a session, it will still show here. To see what people can
        book, check{' '}
        <Link href={`/site/${site.id}/view-availability`}>
          View availability
        </Link>
        .
      </p>
      <p>Any sessions that ended in the past are hiden.</p>

      <br />
      <Suspense fallback={<Spinner />}>
        <AvailabilityCreatedEventsTable siteId={site.id} />
      </Suspense>

      <br />
    </>
  );
};
