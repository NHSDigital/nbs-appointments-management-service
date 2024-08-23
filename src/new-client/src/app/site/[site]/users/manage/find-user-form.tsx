/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { EMAIL_REGEX } from '../../../../../constants';
import {
  TextInput,
  FormGroup,
  Button,
  ButtonGroup,
} from '@nhsuk-frontend-components';

type FormFields = {
  email: string;
};

const FindUserForm = ({ site }: { site: string }) => {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { replace } = useRouter();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>({
    defaultValues: { email: '' },
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
          {...register('email', {
            required: true,
            pattern: EMAIL_REGEX,
          })}
        ></TextInput>
      </FormGroup>

      <ButtonGroup>
        <Button type="submit">Search user</Button>
        <Button styleType="secondary" onClick={cancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default FindUserForm;
