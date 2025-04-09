import { UserPageProps } from './page';
import { RoleAssignment } from '@types';
import UserDetailsForm from './user-details-form';
import React from 'react';
import { fetchRoles, fetchUsers } from '@services/appointmentsService';

const UserDetails = async ({ params, searchParams }: UserPageProps) => {
  const user = searchParams?.user;

  if (user === undefined)
    throw Error('User with specified email address not found');

  const [roles, users] = await Promise.all([
    fetchRoles(),
    fetchUsers(params.site),
  ]);
  const currentUser = users.find(usr => usr.id === user);
  const currentUserAssignments =
    currentUser?.roleAssignments ?? ([] as RoleAssignment[]);

  return (
    <UserDetailsForm
      user={user}
      roles={roles}
      assignments={currentUserAssignments}
      site={params.site}
      firstName={currentUser?.firstName as string}
      lastName={currentUser?.lastName as string}
      isEdit={!!currentUser}
    />
  );
};

export default UserDetails;
