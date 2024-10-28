'use client';
import {
  Button,
  ButtonGroup,
  FormGroup,
  TextArea,
} from '@components/nhsuk-frontend';
import { setSiteInformationForCitizen } from '@services/appointmentsService';
import { SetAttributesRequest } from '@types';
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
      informationForCitizen: information,
    },
  });

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetAttributesRequest = {
      scope: 'site_details',
      attributeValues: [
        {
          id: 'site_details/info_for_citizen',
          value: form.informationForCitizen,
        },
      ],
    };

    await setSiteInformationForCitizen(site, payload);

    replace(`/site/${site}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup legend="Information for citizens">
        <div className="nhsuk-form-group">
          <TextArea
            label="What information would you like to include?"
            {...register('informationForCitizen')}
          />
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
