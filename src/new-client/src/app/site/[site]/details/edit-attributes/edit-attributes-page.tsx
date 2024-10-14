import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';
import AddAttributesForm from './add-attributes-form';

type Props = {
  site: string;
  permissions: string[];
};

export const SiteAttributesPage = async ({ site }: Props) => {
  const attributeDefinitions = await fetchAttributeDefinitions();
  const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
    ad.id.startsWith('accessibility'),
  );
  const siteAttributeValues = await fetchSiteAttributeValues(site);

  return (
    <>
      <div className="nhsuk-form-group">
        <div className="nhsuk-hint">Configure your current site details</div>
      </div>
      <AddAttributesForm
        attributeDefinitions={accessibilityAttributeDefinitions}
        site={site}
        attributeValues={siteAttributeValues}
      />
    </>
  );
};
