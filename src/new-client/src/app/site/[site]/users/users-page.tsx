import { Table } from '@nhsuk-frontend-components';
import Link from 'next/link';
import { Role, User } from '@types';

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

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'baseline' }}>
        <span className="nhsuk-hint" style={{ flexGrow: '1' }}>
          Manage your current site's staff roles
        </span>
        {permissions.includes('users:manage') && (
          <span>
            <AddRoleAssignmentsButton />
          </span>
        )}
      </div>
      <Table
        headers={[
          'Email',
          'Roles',
          ...[permissions.includes('users:manage') ? ['Manage'] : []],
        ]}
        rows={users.map(user => {
          return [
            user.id,
            user.roleAssignments
              .filter(ra => isVisibleRole(ra.role))
              ?.map(ra => getRoleName(ra.role))
              ?.join(' | '),
            ...[
              permissions.includes('users:manage')
                ? [<EditRoleAssignmentsButton key={user.id} user={user.id} />]
                : [],
            ],
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
