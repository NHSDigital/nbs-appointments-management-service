import { fetchInformationForCitizens } from '@services/appointmentsService';
import AddInformationForCitizensForm from './add-information-for-citizens-form';

type Props = {
  site: string;
  permissions: string[];
};

export const EditInformationForCitizensPage = async ({ site }: Props) => {
  const informationForCitizens = await fetchInformationForCitizens();

  return (
    <>
      <div className="nhsuk-form-group">
        <div className="nhsuk-hint">
          Configure the information you wish to display to citizens about the
          site
        </div>
      </div>
      <AddInformationForCitizensForm
        information={informationForCitizens}
        site={site}
      />
    </>
  );
};
