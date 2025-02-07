import { RoleAssignment } from '@types';
import AssignRolesForm from './assign-roles-form';
import React from 'react';
import { fetchRoles, fetchUsers } from '@services/appointmentsService';

type AssignRolesProps = {
  site: string;
  user?: string;
};

const AssignRoles = async ({ site, user }: AssignRolesProps) => {
  if (user === undefined || !user.endsWith('@nhs.net'))
    throw Error('You must specify a valid NHS email address');

  const [roles, users] = await Promise.all([fetchRoles(), fetchUsers(site)]);

  const currentUserAssignments =
    users.find(usr => usr.id === user)?.roleAssignments ??
    ([] as RoleAssignment[]);
  return (
    <>
      <div className="nhsuk-form-group">
        <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--m">
          <h1 className="nhsuk-fieldset__heading">Email</h1>
        </legend>
        <div className="nhsuk-label">{user}</div>
      </div>
      <AssignRolesForm
        user={user}
        roles={roles}
        assignments={currentUserAssignments}
        site={site}
      />
    </>
  );
};

export default AssignRoles;
