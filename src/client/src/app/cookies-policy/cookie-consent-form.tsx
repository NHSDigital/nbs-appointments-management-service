'use client';

import FormWrapper from '@components/form-wrapper';
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
import { useForm } from 'react-hook-form';

type CookieConsentFormProps = {
  consentCookie?: NhsMyaCookieConsent;
};

type CookieConsentFormData = {
  consented: 'yes' | 'no';
};

const CookieConsentForm = ({ consentCookie }: CookieConsentFormProps) => {
  const {
    register,
    handleSubmit,
    setError,
    formState: { isSubmitting, isSubmitSuccessful, errors },
  } = useForm<CookieConsentFormData>({
    defaultValues: { consented: consentCookie?.consented ? 'yes' : 'no' },
  });

  const router = useRouter();

  return (
    <FormWrapper<CookieConsentFormData>
      submitHandler={async form => {
        await setCookieConsent(form.consented === 'yes');
        router.push(`/sites`);
      }}
      handleSubmit={handleSubmit}
      setError={setError}
      errors={errors}
    >
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
            id="consented-yes"
            value="yes"
            {...register('consented', {
              required: { value: true, message: 'Select an option' },
            })}
          />
          <Radio
            label="Do not use cookies to measure my website use"
            id="consented-no"
            value="no"
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
    </FormWrapper>
  );
};

export default CookieConsentForm;
