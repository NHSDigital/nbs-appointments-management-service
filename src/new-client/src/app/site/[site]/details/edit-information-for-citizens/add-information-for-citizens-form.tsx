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
  const { register, handleSubmit, formState } = useForm<FormFields>({
    defaultValues: {
      informationForCitizen: information,
    },
  });
  const { errors } = formState;

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

  const isValidTextInput = (text: string): boolean => {
    const urlRegex = new RegExp(
      '([a-zA-Z0-9]+://)?([a-zA-Z0-9_]+:[a-zA-Z0-9_]+@)?([a-zA-Z0-9.-]+\\.[A-Za-z]{2,4})(:[0-9]+)?([^ ])+',
    );
    if (urlRegex.test(text)) {
      return false;
    }

    const specialCharacterRegex = /^[-\w \.\,\-]+$/;
    return specialCharacterRegex.test(text);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        legend="Information for citizens"
        error={errors.informationForCitizen?.message}
      >
        <div className="nhsuk-form-group">
          <TextArea
            label="What information would you like to include?"
            maxLength={150}
            {...register('informationForCitizen', {
              validate: value => {
                if (!isValidTextInput(value)) {
                  return "Text cannot contain a URL or special characters outside of '.' ',' '-'";
                }
              },
            })}
          />
        </div>
      </FormGroup>
      <br />
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
