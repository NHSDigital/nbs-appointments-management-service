'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Role, RoleAssignment } from '@types';
import { saveUserRoleAssignments } from '../../../../lib/users';

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
  const { register, handleSubmit } = useForm<FormFields>({
    defaultValues: {
      roles: assignments.map(a => a.role),
    },
  });

  const submitForm: SubmitHandler<FormFields> = async form => {
    await saveUserRoleAssignments(site, user, form.roles);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <fieldset className="nhsuk-fieldset">
        <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
          <h1 className="nhsuk-fieldset__heading">Roles</h1>
        </legend>
        <div className="nhsuk-checkboxes">
          {roles.map(r => (
            <div key={r.id} className="nhsuk-checkboxes__item">
              <input
                id={r.id}
                type="checkbox"
                className="nhsuk-checkboxes__input"
                value={r.id}
                {...register('roles')}
              />
              <label
                htmlFor={r.id}
                className="nhsuk-label nhsuk-checkboxes__label"
              >
                {r.displayName}
              </label>
              <div
                className="nhsuk-hint nhsuk-checkboxes__hint"
                id="nationality-1-item-hint"
              >
                {r.description}
              </div>
            </div>
          ))}
        </div>
      </fieldset>

      <div style={{ marginTop: '20px' }}>
        <div className="nhsuk-navigation">
          <button
            type="submit"
            aria-label="save user"
            className="nhsuk-button nhsuk-u-margin-bottom-0"
          >
            Confirm and save
          </button>
          <button
            type="button"
            aria-label="cancel"
            className="nhsuk-button nhsuk-button--secondary nhsuk-u-margin-left-3 nhsuk-u-margin-bottom-0"
          >
            Cancel
          </button>
        </div>
      </div>
    </form>
  );
};

export default AssignRolesForm;
