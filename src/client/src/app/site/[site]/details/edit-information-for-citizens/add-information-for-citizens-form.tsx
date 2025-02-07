'use client';
import {
  Button,
  ButtonGroup,
  FormGroup,
  SmallSpinnerWithText,
  TextArea,
} from '@components/nhsuk-frontend';
import { setSiteInformationForCitizen } from '@services/appointmentsService';
import { SetInformationForCitizensRequest } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { SPECIAL_CHARACTER_REGEX, URL_REGEX } from '../../../../../constants';

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
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
    watch,
  } = useForm<FormFields>({
    defaultValues: {
      informationForCitizen: information,
    },
  });

  const infoWatch = watch('informationForCitizen');
  const maxLength = 150;

  const cancel = () => {
    replace(`/site/${site}/details`);
  };

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetInformationForCitizensRequest = {
      informationForCitizens: form.informationForCitizen,
    };

    await setSiteInformationForCitizen(site, payload);

    replace(`/site/${site}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        legend="Information for citizens"
        error={
          errors.informationForCitizen
            ? 'Site information cannot contain a URL or special characters except full stops, commas, and hyphens'
            : ''
        }
      >
        <div className="nhsuk-form-group">
          <TextArea
            label="What information would you like to include?"
            maxLength={maxLength}
            {...register('informationForCitizen', {
              validate: {
                validInput: value =>
                  value.length === 0 ||
                  (!URL_REGEX.test(value) &&
                    SPECIAL_CHARACTER_REGEX.test(value)),
              },
              maxLength: 150,
            })}
          />
        </div>
        <div
          className="nhsuk-hint nhsuk-character-count__message"
          aria-label="information-characters-remaining"
        >
          You have {maxLength - infoWatch.length} characters remaining
        </div>
      </FormGroup>
      <br />

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Confirm site details</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default AddInformationForCitizensForm;
