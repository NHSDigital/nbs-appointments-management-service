'use client';
import { Button, ButtonGroup, FormGroup } from '@components/nhsuk-frontend';
import { setSiteInformationForCitizen } from '@services/appointmentsService';
import { AttributeValue } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type FormFields = {
  informationForCitizen: string;
};

const AddInformationForCitizensForm = ({
  information,
  site,
}: {
  information: string;
  site: string;
}) => {
  const { replace } = useRouter();
  const { register, handleSubmit } = useForm<FormFields>({
    defaultValues: {
      informationForCitizen: '',
    },
  });

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: AttributeValue[] = [
      {
        id: 'info_for_citizen',
        value: form.informationForCitizen,
      },
    ];

    await setSiteInformationForCitizen(site, payload);

    replace(`/site/${site}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup legend="Information for citizen">
        <div className="nhsuk-form-group">
          <div className="nhsuk-label">
            What information would you like to include?
          </div>
          <div className="nhsuk-hint" id="information-hint">
            Do not include personal or financial information, for example, your
            National Insurance number or credit card details.
          </div>
          {/* TODO: Make this it's own component */}
          <textarea
            className="nhsuk-textarea"
            id="information-for-citzen"
            rows={5}
            aria-describedby="information-hint"
            value={information}
          ></textarea>
        </div>
      </FormGroup>

      <ButtonGroup>
        <Button type="submit">Confirm site details</Button>
        <Button styleType="secondary" onClick={cancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default AddInformationForCitizensForm;
