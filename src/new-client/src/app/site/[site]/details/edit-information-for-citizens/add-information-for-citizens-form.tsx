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
import { useState } from 'react';
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
  const [textInputLength, setTextInputLength] = useState(0);
  const maxLength = 150;

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

  const handleTextInputUpdate = (inputLength: number): void => {
    setTextInputLength(inputLength);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <div
        className="nhsuk-character-count"
        data-module="nhsuk-character-count"
        data-maxlength="150"
      >
        <FormGroup
          legend="Information for citizens"
          error={errors.informationForCitizen?.message}
        >
          <div className="nhsuk-form-group">
            <TextArea
              label="What information would you like to include?"
              maxLength={maxLength}
              {...register('informationForCitizen', {
                validate: value => {
                  if (!isValidTextInput(value)) {
                    return "Text cannot contain a URL or special characters outside of '.' ',' '-'";
                  }
                },
                onChange: e => {
                  handleTextInputUpdate(e.target.value.length);
                },
              })}
            />
          </div>
          <div
            className="nhsuk-hint nhsuk-character-count__message"
            id="more-detail-info"
          >
            You have {maxLength - textInputLength} characters remaining
          </div>
        </FormGroup>
      </div>
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
