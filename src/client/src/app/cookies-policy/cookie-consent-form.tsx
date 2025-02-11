'use client';

import {
  Button,
  FormGroup,
  Radio,
  RadioGroup,
  SmallSpinnerWithText,
} from '@components/nhsuk-frontend';
import { setCookieConsent } from '@services/cookiesService';
import { NhsMyaCookieConsent } from '@types';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';

type CookieConsentFormProps = {
  consentCookie?: NhsMyaCookieConsent;
};

type CookieConsentFormData = {
  consented: boolean;
};

const CookieConsentForm = ({ consentCookie }: CookieConsentFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<CookieConsentFormData>({
    defaultValues: { consented: consentCookie?.consented },
  });

  const router = useRouter();
  const submitForm: SubmitHandler<CookieConsentFormData> = async (
    form: CookieConsentFormData,
  ) => {
    await setCookieConsent(form.consented);
    router.push(`/`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <p>
        We'll only use these cookies if you say it's okay. We'll use a cookie to
        save your settings.
      </p>

      <FormGroup
        legend="Tell us if we can use analytics cookies"
        error={errors.consented?.message}
      >
        <RadioGroup>
          <Radio
            label="Use cookies to measure my website use"
            value="true"
            {...register('consented', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label="Do not use cookies to measure my website use"
            value="false"
            {...register('consented', {
              required: { value: true, message: 'Select an option' },
            })}
          />
        </RadioGroup>
      </FormGroup>
      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Save my cookie settings</Button>
      )}
    </form>
  );
};

export default CookieConsentForm;
