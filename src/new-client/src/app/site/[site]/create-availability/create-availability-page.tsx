import { Button } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Site } from '@types';

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
      <Link href={`/site/${site.id}/create-availability/wizard`}>
        <Button type="button">Create availablity</Button>
      </Link>
    </>
  );
};
