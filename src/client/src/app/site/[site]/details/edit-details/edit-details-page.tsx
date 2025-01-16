import { fetchSite } from '@services/appointmentsService';
import EditDetailsForm from './edit-details-form';

type Props = {
  site: string;
  permissions: string[];
};

export const EditDetailsPage = async ({ site }: Props) => {
  const siteDetails = await fetchSite(site);

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <div className="nhsuk-hint">Edit site details</div>
        </div>
        <EditDetailsForm siteWithAttributes={siteDetails}></EditDetailsForm>
      </div>
    </div>
  );
};
