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

const schema = yup
  .object({
    email: yup
      .string()
      .required()
      .trim()
      .lowercase()
      .email()
      // TODO: Toggle or remove this to permit Okta user creation
      .test(
        'is-nhs-email',
        'You have not entered a valid NHS email address',
        email => email.endsWith('@nhs.net'),
      ),
  })
  .required();

const FindUserForm = ({ site }: { site: string }) => {
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
      params.set('user', form.email);
    } else {
      params.delete('user');
    }
    replace(`${pathname}?${params.toString()}`);
  };

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        error={
          errors.email
            ? 'You have not entered a valid NHS email address'
            : undefined
        }
      >
        <TextInput
          id="email"
          label="Enter an email address"
          {...register('email')}
        ></TextInput>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Searching for user..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Search user</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default FindUserForm;
