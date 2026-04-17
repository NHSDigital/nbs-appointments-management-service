import { Button } from 'nhsuk-react-components';
import Link from 'next/link';
import { Role, User, UserProfile } from '@types';
import { useMemo } from 'react';
import { UsersPageTableData } from './users-page-table-data';

type Props = {
  userProfile: UserProfile;
  users: User[];
  roles: Role[];
  permissions: string[];
};

export const UsersPage = ({
  userProfile,
  users,
  roles,
  permissions,
}: Props) => {
  const canSeeAdminControls = useMemo(() => {
    return permissions.includes('users:manage');
  }, [permissions]);

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
        {canSeeAdminControls === true && (
          <span>
            <AddRoleAssignmentsButton />
          </span>
        )}
      </div>
      <UsersPageTableData
        users={users}
        canSeeAdminControls={canSeeAdminControls}
        userProfileEmail={userProfile.emailAddress}
        roles={roles}
      />
    </>
  );
};

const AddRoleAssignmentsButton = () => (
  <div style={{ fontSize: 'large' }}>
    <Link href={`users/manage`} className="nhsuk-link">
      <Button type="button">Add user</Button>
    </Link>
  </div>
);
