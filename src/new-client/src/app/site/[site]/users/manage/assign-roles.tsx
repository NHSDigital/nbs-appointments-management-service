import { UserPageProps } from './page';
import { fetchRoles } from '../../../../lib/roles';
import { fetchUsers } from '../../../../lib/users';
import { RoleAssignment } from '@types';
import AssignRolesForm from './assign-roles-form';
import React from 'react';

const AssignRoles = async ({ params, searchParams }: UserPageProps) => {
  const user = searchParams?.user;

  if (user === undefined || !user.endsWith('@nhs.net'))
    throw Error('You must specify a valid NHS email address');

  const roles = await fetchRoles();
  const users = await fetchUsers(params.site);

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
        site={params.site}
      />
    </>
  );
};

export default AssignRoles;
