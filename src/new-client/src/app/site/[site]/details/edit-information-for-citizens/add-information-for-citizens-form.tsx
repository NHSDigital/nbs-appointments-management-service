import { Button, ButtonGroup, FormGroup } from '@components/nhsuk-frontend';
import { useRouter } from 'next/router';
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

  console.log('Add info page hit.');

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    alert('submit button clicked!');
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
          <textarea
            className="nhsuk-textarea"
            id="information-for-citzen"
            name="information-for-citizen"
            rows={5}
            aria-describedby="information-hint"
          >
            {information}
          </textarea>
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
