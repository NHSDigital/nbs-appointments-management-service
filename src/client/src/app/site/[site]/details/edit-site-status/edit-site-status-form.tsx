import {
  Button,
  FormGroup,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { updateSiteStatus } from '@services/appointmentsService';
import { Site, SiteStatus } from '@types';
import { useRouter } from 'next/router';
import { SubmitHandler, useForm } from 'react-hook-form';

type FormFields = {
  siteStatus: SiteStatus;
};

const EditSiteStatusForm = ({ site }: { site: Site }) => {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<FormFields>({
    defaultValues: {
      siteStatus: site.status ?? 'Online',
    },
  });

  const { replace } = useRouter();

  const onlineLabel =
    site.status === 'Online' || undefined
      ? 'Keep site online'
      : 'Make site online';
  const offlineLabel =
    site.status === 'Offline' ? 'Keep site offline' : 'Take site offline';

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    await updateSiteStatus(site.id, form.siteStatus);

    replace(`/site/${site.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        legend="What do you want to do?"
        error={errors.siteStatus?.message}
      >
        <RadioGroup>
          <Radio
            label={onlineLabel}
            id="site-status-online"
            value="Online"
            {...register('siteStatus', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label={offlineLabel}
            id="site-status-offline"
            value="Offline"
            {...register('siteStatus', {
              required: { value: true, message: 'Select an option' },
            })}
          />
        </RadioGroup>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Save and continue</Button>
      )}
    </form>
  );
};

export default EditSiteStatusForm;
