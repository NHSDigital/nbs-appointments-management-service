'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Role, RoleAssignment } from '@types';
import { saveUserRoleAssignments } from '../../../../lib/users';
import { useRouter } from 'next/navigation';

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

  const submitForm: SubmitHandler<FormFields> = async form => {
    await saveUserRoleAssignments(site, user, form.roles);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <div
        className={`nhsuk-form-group ${errors.roles ? 'nhsuk-form-group--error' : ''}`}
      >
        <fieldset className="nhsuk-fieldset">
          <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
            <h1 className="nhsuk-fieldset__heading">Roles</h1>
          </legend>
          {errors.roles && (
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error:</span> You have
              not selected any roles for this user
            </span>
          )}
          <div className="nhsuk-checkboxes">
            {roles.map(r => (
              <div key={r.id} className="nhsuk-checkboxes__item">
                <input
                  id={r.id}
                  type="checkbox"
                  className="nhsuk-checkboxes__input"
                  value={r.id}
                  {...register('roles', { required: true })}
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
      </div>

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
            onClick={cancel}
          >
            Cancel
          </button>
        </div>
      </div>
    </form>
  );
};

export default AssignRolesForm;
