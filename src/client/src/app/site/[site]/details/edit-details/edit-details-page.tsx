import { fetchSite } from '@services/appointmentsService';
import EditDetailsForm from './edit-details-form';
import NhsHeading from '@components/nhs-heading';

type Props = {
  siteId: string;
};

export const EditDetailsPage = async ({ siteId }: Props) => {
  const siteDetails = await fetchSite(siteId);

  return (
    <>
      <NhsHeading title="Edit site details" caption={siteDetails.name} />
      <EditDetailsForm site={siteDetails}></EditDetailsForm>
    </>
  );
};
