'use client';
import {
  Button,
  FormGroup,
  InsetText,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import fromServer from '@server/fromServer';
import { updateSiteStatus } from '@services/appointmentsService';
import { Site, SiteStatus } from '@types';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';

type FormFields = {
  siteStatus: SiteStatus;
};

const EditSiteStatusForm = ({ site }: { site: Site }) => {
  const [pendingSubmit, startTransition] = useTransition();
  const {
    register,
    handleSubmit,
    formState: { errors },
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
  const statusLabel =
    site.status === 'Online' || undefined
      ? 'Patients can currently book appointments at this site'
      : 'Patients can not currently book appointments at this site';

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    startTransition(async () => {
      await fromServer(updateSiteStatus(site.id, form.siteStatus));

      replace(`/site/${site.id}/details`);
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <p>{statusLabel}</p>
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
        <InsetText>
          <p>
            The change will take effect immediately. Taking your site offline
            will mean patients can no longer book appointments until the site is
            online again. This will not affect existing bookings.
          </p>
        </InsetText>
      </FormGroup>

      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Save and continue</Button>
      )}
    </form>
  );
};

export default EditSiteStatusForm;
