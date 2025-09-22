import {
  fetchAccessibilityDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import AddAccessibilitiesForm from './add-accessibilities-form';
import fromServer from '@server/fromServer';

type Props = {
  site: string;
  permissions: string[];
};

const EditAccessibilitiesPage = async ({ site }: Props) => {
  const AccessibilityDefinitions = await fromServer(
    fetchAccessibilityDefinitions(),
  );
  const accessibilityAccessibilityDefinitions = AccessibilityDefinitions.filter(
    ad => ad.id.startsWith('accessibility'),
  );
  const siteDetails = await fromServer(fetchSite(site));

  return (
    <>
      <div className="nhsuk-form-group">
        <div className="nhsuk-hint">Configure your current site details</div>
      </div>
      <AddAccessibilitiesForm
        accessibilityDefinitions={accessibilityAccessibilityDefinitions}
        site={site}
        accessibilities={siteDetails?.accessibilities ?? []}
      />
    </>
  );
};

export default EditAccessibilitiesPage;
