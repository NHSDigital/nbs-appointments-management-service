import { Button } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Site } from '@types';
// import { fetchAvailabilityPeriods } from '@services/appointmentsService';

type Props = {
  site: Site;
};

export const CreateAvailabilityPage = async ({ site }: Props) => {
  // TODO: get this from the appointmentsService once implemented
  // const availabilityPeriods = await fetchAvailabilityPeriods(site.id);

  return (
    <>
      <p>
        You can create and edit availability periods with multiple days and
        repeating sessions, to accurately reflect your site's capacity.
      </p>
      {/* <Table
        headers={['Dates', 'Services', 'Status', 'Actions']}
        rows={availabilityPeriods.map((period, index) => {
          return [
            `${period.startDate.format('DD MMM YYYY')} - ${period.endDate.format('DD MMM YYYY')}`,
            period.services.join('/'),
            <Tag
              text={period.status}
              colour={period.status === 'Published' ? 'blue' : 'grey'}
              key={`availability-period-${index}-status`}
            />,
            <Link
              href={`#`}
              className="nhsuk-link"
              key={`availability-period-${index}-action`}
            >
              Edit
            </Link>,
          ];
        })}
      /> */}
      <br />
      <Link href={`/site/${site.id}/create-availability/wizard`}>
        <Button type="button">Create availablity</Button>
      </Link>
    </>
  );
};
