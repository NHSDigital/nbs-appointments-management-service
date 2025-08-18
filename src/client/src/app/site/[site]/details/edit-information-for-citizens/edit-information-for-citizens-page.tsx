import { fetchInformationForCitizens } from '@services/appointmentsService';
import AddInformationForCitizensForm from './add-information-for-citizens-form';
import { FormGroup } from '@components/nhsuk-frontend';
import fromServer from '@server/fromServer';

type Props = {
  site: string;
  permissions: string[];
};

export const EditInformationForCitizensPage = async ({ site }: Props) => {
  const informationForCitizens = await fromServer(
    fetchInformationForCitizens(site),
  );
  return (
    <FormGroup hint="Configure the information you wish to display to citizens about the site">
      <AddInformationForCitizensForm
        information={informationForCitizens}
        site={site}
      />
    </FormGroup>
  );
};
