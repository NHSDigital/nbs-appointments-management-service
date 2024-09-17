import AddAttributesForm from './AddAttributesForm';
import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';

type Props = {
  site: string;
};
export const ManageAttributesPage = async ({ site }: Props) => {
  const attributeDefinitions = await fetchAttributeDefinitions();
  const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
    ad.id.startsWith('accessibility'),
  );

  const siteAttributeValues = await fetchSiteAttributeValues(site);

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <div className="nhsuk-hint">Configure your current site details</div>
        </div>

        <AddAttributesForm
          attributeDefinitions={accessibilityAttributeDefinitions}
          site={site}
          attributeValues={siteAttributeValues}
        />
      </div>
    </div>
  );
};
