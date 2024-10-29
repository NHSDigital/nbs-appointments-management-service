import { fetchInformationForCitizens } from '@services/appointmentsService';
import AddInformationForCitizensForm from './add-information-for-citizens-form';
import { FormGroup } from '@components/nhsuk-frontend';

type Props = {
  site: string;
  permissions: string[];
};

export const EditInformationForCitizensPage = async ({ site }: Props) => {
  const informationForCitizens = await fetchInformationForCitizens(
    site,
    'site_details',
  );
  const siteInformation = informationForCitizens.find(
    i => i.id === 'site_details/info_for_citizen',
  );

  return (
    <FormGroup hint="Configure the information you wish to display to citizens about the site">
      <AddInformationForCitizensForm
        information={siteInformation?.value ?? ''}
        site={site}
      />
    </FormGroup>
  );
};
