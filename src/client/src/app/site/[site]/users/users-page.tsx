import { Table, Button } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Role, User, UserProfile } from '@types';
import { useMemo } from 'react';

type Props = {
  userProfile: UserProfile;
  users: User[];
  roles: Role[];
  permissions: string[];
  oktaEnabled: boolean;
};

export const UsersPage = ({
  userProfile,
  users,
  roles,
  permissions,
  oktaEnabled,
}: Props) => {
  const isVisibleRole = (role: string) =>
    roles.find(r => r.id === role) !== undefined;
  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;

  const canSeeAdminControls = useMemo(() => {
    return permissions.includes('users:manage');
  }, [permissions]);

  const canEditUser = (email: string): boolean => {
    return email.endsWith('@nhs.net') || oktaEnabled;
  };

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
        {canSeeAdminControls === true && (
          <span>
            <AddRoleAssignmentsButton />
          </span>
        )}
      </div>
      <Table
        headers={[
          'Email',
          'Roles',
          ...(canSeeAdminControls ? ['Manage', 'Remove'] : []),
        ]}
        rows={users.map(user => {
          return [
            user.id,
            user.roleAssignments
              .filter(ra => isVisibleRole(ra.role))
              ?.map(ra => getRoleName(ra.role))
              ?.join(' | '),
            ...(canSeeAdminControls
              ? [
                  ...(userProfile.emailAddress === user.id ||
                  !canEditUser(user.id)
                    ? ['', '']
                    : [
                        <EditRoleAssignmentsButton
                          key={`edit-${user.id}`}
                          user={user.id}
                        />,
                        <RemoveUserButton
                          key={`remove-${user.id}`}
                          user={user.id}
                        />,
                      ]),
                ]
              : []),
          ];
        })}
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

const EditRoleAssignmentsButton = ({ user }: { user: string }) => (
  <div>
    <Link href={`users/manage?user=${user}`} className="nhsuk-link">
      Edit
    </Link>
  </div>
);

const RemoveUserButton = ({ user }: { user: string }) => (
  <div>
    <Link href={`users/remove?user=${user}`} className="nhsuk-link">
      Remove from this site
    </Link>
  </div>
);
