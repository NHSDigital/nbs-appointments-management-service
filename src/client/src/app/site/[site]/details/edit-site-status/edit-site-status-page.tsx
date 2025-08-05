import NhsHeading from '@components/nhs-heading';
import { fetchSite } from '@services/appointmentsService';

type Props = {
  siteId: string;
};

export const EditSiteStatusPage = async ({ siteId }: Props) => {
  const siteDetails = await fetchSite(siteId);

  return (
    <>
      <NhsHeading title="Manage site visibility" caption={siteDetails.name} />
      <span>Hello world!</span>
    </>
  );
};
