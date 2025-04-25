/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { yupResolver } from '@hookform/resolvers/yup';
import { SubmitHandler, useForm } from 'react-hook-form';
import * as yup from 'yup';
import {
  TextInput,
  FormGroup,
  Button,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@nhsuk-frontend-components';

type FormFields = {
  email: string;
};

// TODO: This is a temporary solution to prove the validation works
// Next PR will use the real list and not have them hardcoded here
const allowedEmailDomains = [
  'nhs.net',
  'boots.co.uk',
  'lloydspharmacy.co.uk',
  'mha.org.uk',
  'northerntrust.hscni.net',
  'redcross.org.uk',
  'superdrug.com',
  'okta.net',
];

const FindUserForm = ({
  site,
  oktaEnabled,
}: {
  site: string;
  oktaEnabled: boolean;
}) => {
  const schema = yup
    .object({
      email: yup
        .string()
        .required()
        .trim()
        .lowercase()
        .email()
        .test(
          'is-allowed-email-domain',
          'Email address must be @nhs.net or an authorised email domain',
          email =>
            oktaEnabled
              ? allowedEmailDomains.includes(email.split('@')[1])
              : email.endsWith('@nhs.net'),
        ),
    })
    .required();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: { email: '' },
    resolver: yupResolver(schema),
  });

  const submitForm: SubmitHandler<FormFields> = form => {
    const params = new URLSearchParams(searchParams);
    if (form.email) {
      params.set('user', form.email.toLowerCase());
    } else {
      params.delete('user');
    }
    replace(`${pathname}?${params.toString()}`);
  };

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  return (
    <>
      <h2>Add a user</h2>
      <>
        Email address must be nhs.net or on the list of{' '}
        <a href="https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/care-identity-email-domain-allow-list">
          Authorised email domains.
        </a>{' '}
        Read the{' '}
        <a href="https://digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance/log-in-and-select-site">
          user guidance on logging in without an NHS.net account
        </a>{' '}
        or you can apply for their{' '}
        <a href="https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/request-an-addition-to-the-email-domain-allow-list">
          email domain
        </a>
      </>
      <form onSubmit={handleSubmit(submitForm)}>
        <br />
        <FormGroup
          error={
            errors.email
              ? 'Email address must be nhs.net or an authorised email domain'
              : undefined
          }
        >
          <TextInput
            id="email"
            label="Enter email address"
            {...register('email')}
          ></TextInput>
        </FormGroup>

        {isSubmitting || isSubmitSuccessful ? (
          <SmallSpinnerWithText text="Searching for user..." />
        ) : (
          <ButtonGroup>
            <Button type="submit">Continue</Button>
            <Button styleType="secondary" onClick={cancel}>
              Cancel
            </Button>
          </ButtonGroup>
        )}
      </form>
    </>
  );
};

export default FindUserForm;
