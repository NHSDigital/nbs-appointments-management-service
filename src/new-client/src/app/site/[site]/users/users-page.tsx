import { Table } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Role, User } from '@types';
import { useMemo } from 'react';

type Props = {
  users: User[];
  roles: Role[];
  permissions: string[];
};

export const UsersPage = ({ users, roles, permissions }: Props) => {
  const isVisibleRole = (role: string) =>
    roles.find(r => r.id === role) !== undefined;
  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;

  const canSeeAdminControls = useMemo(() => {
    return permissions.includes('users:manage');
  }, [permissions]);

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'baseline' }}>
        <span className="nhsuk-hint" style={{ flexGrow: '1' }}>
          Manage your current site's staff roles
        </span>
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
                  <EditRoleAssignmentsButton
                    key={`edit-${user.id}`}
                    user={user.id}
                  />,
                  <RemoveUserButton key={`remove-${user.id}`} user={user.id} />,
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
      Assign staff roles
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
