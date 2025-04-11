/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Role, RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
  TextInput,
} from '@nhsuk-frontend-components';
import { sortRolesByName } from '@sorting';
import { When } from '@components/when';

type FormFields = {
  roles: string[];
  firstName: string;
  lastName: string;
};

const UserDetailsForm = ({
  site,
  user,
  roles,
  assignments,
  firstName,
  lastName,
  isEdit,
}: {
  site: string;
  user: string;
  roles: Role[];
  assignments: RoleAssignment[];
  firstName: string;
  lastName: string;
  isEdit: boolean;
}) => {
  const { replace, push } = useRouter();
  const sortedRoles = roles.toSorted(sortRolesByName);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormFields>({
    defaultValues: { roles: assignments.map(a => a.role) },
  });
  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  const submitForm: SubmitHandler<FormFields> = form => {
    const dataToPass = {
      site,
      user,
      roles: form.roles,
      firstName: form.firstName,
      lastName: form.lastName,
      isEdit,
    };

    // Store in sessionStorage to avoid large query params
    sessionStorage.setItem('userFormData', JSON.stringify(dataToPass));

    // Navigate to the summary page
    push(`/site/${site}/users/user-summary`);
  };

  return (
    <>
      <h2>User details</h2>

      <form onSubmit={handleSubmit(submitForm)}>
        <When condition={!user.endsWith('nhs.net')}>
          <FormGroup
            error={errors.firstName ? 'FirstName is mandatory' : undefined}
          >
            <TextInput
              id="firstName"
              label="First name"
              value={firstName}
              {...register('firstName', {
                required: !user.endsWith('nhs.net'),
              })}
            ></TextInput>
          </FormGroup>

          <FormGroup
            error={errors.firstName ? 'LastName is mandatory' : undefined}
          >
            <TextInput
              id="lastName"
              label="Last name"
              value={lastName}
              {...register('lastName', {
                required: !user.endsWith('nhs.net'),
              })}
            ></TextInput>
          </FormGroup>
        </When>

        <FormGroup legend="Email">
          <span style={{ fontSize: '20px' }}>{user}</span>
        </FormGroup>

        <FormGroup
          error={
            errors.roles
              ? 'You have not selected any roles for this user'
              : undefined
          }
          legend="Roles"
        >
          <CheckBoxes>
            {sortedRoles.map(r => (
              <CheckBox
                id={r.id}
                label={r.displayName}
                hint={r.description}
                key={`checkbox-key-${r.id}`}
                value={r.id}
                {...register('roles', { required: true })}
              />
            ))}
          </CheckBoxes>
        </FormGroup>

        <ButtonGroup>
          <Button type="submit">Continue</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      </form>
    </>
  );
};

export default UserDetailsForm;
