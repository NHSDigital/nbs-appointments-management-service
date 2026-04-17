'use client';

import { FormGroup, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import { setCookieConsent } from '@services/cookiesService';
import { NhsMyaCookieConsent } from '@types';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Button, Radios } from 'nhsuk-react-components';

type CookieConsentFormProps = {
  consentCookie?: NhsMyaCookieConsent;
};

type CookieConsentFormData = {
  consented: 'yes' | 'no';
};

const CookieConsentForm = ({ consentCookie }: CookieConsentFormProps) => {
  const [pendingSubmit, startTransition] = useTransition();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CookieConsentFormData>({
    defaultValues: { consented: consentCookie?.consented ? 'yes' : 'no' },
  });

  const router = useRouter();
  const submitForm: SubmitHandler<CookieConsentFormData> = async (
    form: CookieConsentFormData,
  ) => {
    startTransition(async () => {
      await setCookieConsent(form.consented === 'yes');
      router.push(`/sites`);
    });
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
        <Radios>
          <Radios.Item
            value="yes"
            id="consented-yes"
            {...register('consented', {
              required: 'Select an option',
            })}
          >
            Use cookies to measure my website use
          </Radios.Item>
          <Radios.Item value="no" id="consented-no" {...register('consented')}>
            Do not use cookies to measure my website use
          </Radios.Item>
        </Radios>
      </FormGroup>
      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button type="submit">Save my cookie settings</Button>
      )}
    </form>
  );
};

export default CookieConsentForm;
