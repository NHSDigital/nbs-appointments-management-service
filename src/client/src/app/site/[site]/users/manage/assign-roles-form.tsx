/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Role, RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import {
  Button,
  FormGroup,
  CheckBoxes,
  CheckBox,
  ButtonGroup,
  SmallSpinnerWithText,
} from '@nhsuk-frontend-components';
import { sortRolesByName } from '@sorting';

type FormFields = {
  roles: string[];
};

const AssignRolesForm = ({
  site,
  user,
  roles,
  assignments,
}: {
  site: string;
  user: string;
  roles: Role[];
  assignments: RoleAssignment[];
}) => {
  const { replace } = useRouter();
  const sortedRoles = roles.toSorted(sortRolesByName);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: { roles: assignments.map(a => a.role) },
  });

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  const submitForm: SubmitHandler<FormFields> = async form => {
    await saveUserRoleAssignments(site, user, form.roles);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
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

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Saving..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Confirm and save</Button>
          <Button styleType="secondary" onClick={cancel}>
            Cancel
          </Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default AssignRolesForm;
