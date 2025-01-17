import { fetchSite } from '@services/appointmentsService';
import EditDetailsForm from './edit-details-form';
import NhsHeading from '@components/nhs-heading';

type Props = {
  site: string;
};

export const EditDetailsPage = async ({ site }: Props) => {
  const siteDetails = await fetchSite(site);

  return (
    <>
      <NhsHeading title="Edit site details" caption={siteDetails.name} />
      <EditDetailsForm siteWithAttributes={siteDetails}></EditDetailsForm>
    </>
  );
};
