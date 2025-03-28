﻿import { UserPageProps } from './page';
import { RoleAssignment } from '@types';
import AssignRolesForm from './assign-roles-form';
import React from 'react';
import {
  fetchFeatureFlag,
  fetchRoles,
  fetchUsers,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';

const AssignRoles = async ({ params, searchParams }: UserPageProps) => {
  const user = searchParams?.user?.toLowerCase();

  if (user === undefined) {
    return notFound();
  }

  const oktaLoginFlag = await fetchFeatureFlag('OktaLogin');
  if (!oktaLoginFlag.enabled && !user.includes('@nhs.net')) {
    return notFound();
  }

  const [roles, users] = await Promise.all([
    fetchRoles(),
    fetchUsers(params.site),
  ]);

  const currentUser = users.find(usr => usr.id === user);
  const currentUserAssignments =
    currentUser?.roleAssignments ?? ([] as RoleAssignment[]);

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
        firstName={currentUser?.firstName as string}
        lastName={currentUser?.lastName as string}
      />
    </>
  );
};

export default AssignRoles;
