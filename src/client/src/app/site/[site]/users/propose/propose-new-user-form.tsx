/* eslint-disable react/jsx-props-no-spreading */
'use client';
import { useRouter } from 'next/navigation';
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
import { proposeNewUser } from '@services/appointmentsService';

type FormFields = {
  email: string;
};

const ProposeNewUserForm = ({ site }: { site: string }) => {
  const schema = yup
    .object({
      email: yup
        .string()
        .required('Enter a valid email address')
        .trim()
        .lowercase()
        .email('Enter a valid email address'),
    })
    .required();

  const { push } = useRouter();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: { email: '' },
    resolver: yupResolver(schema),
  });

  const submitForm: SubmitHandler<FormFields> = async form => {
    const proposedUser = await proposeNewUser(site, form.email);

    if (proposedUser.extantInMya) {
      push(`/site/${site}/users/edit?user=${form.email}`);
      return;
    }

    if (!proposedUser.meetsWhitelistRequirements) {
      setError('email', { message: 'Enter a valid email address' });
      return;
    }

    const nameRequired =
      proposedUser.identityProvider === 'Okta' &&
      proposedUser.extantInIdentityProvider === false;

    push(
      `/site/${site}/users/${form.email}/manage?nameRequired=${nameRequired}`,
    );
  };

  const cancel = () => {
    push(`/site/${site}/users`);
  };

  return (
    <>
      <h2>Add a user</h2>
      <form onSubmit={handleSubmit(submitForm)}>
        <FormGroup error={errors?.email?.message}>
          <TextInput
            id="email"
            label="Enter email address"
            {...register('email')}
          />
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

export default ProposeNewUserForm;
