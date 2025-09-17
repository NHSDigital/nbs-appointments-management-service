import {
  fetchSite,
  fetchWellKnownOdsCodeEntries,
} from '@services/appointmentsService';
import EditReferenceDetailsForm from './edit-reference-details-form';
import NhsHeading from '@components/nhs-heading';
import fromServer from '@server/fromServer';

type Props = {
  siteId: string;
};

export const EditReferenceDetailsPage = async ({ siteId }: Props) => {
  const [siteDetails, wellKnownOdsCodeEntries] = await Promise.all([
    fromServer(fetchSite(siteId)),
    fromServer(fetchWellKnownOdsCodeEntries()),
  ]);

  return (
    <>
      <NhsHeading
        title="Edit site reference details"
        caption={siteDetails.name}
      />
      <EditReferenceDetailsForm
        site={siteDetails}
        wellKnownOdsCodeEntries={wellKnownOdsCodeEntries}
      ></EditReferenceDetailsForm>
    </>
  );
};
