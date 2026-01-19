'use client';
import { UserProfile, User, Role } from '@types';
import Link from 'next/link';
import { Button, Table } from 'nhsuk-react-components';
import { useMemo } from 'react';

type Props = {
  userProfile: UserProfile;
  users: User[];
  roles: Role[];
  permissions: string[];
  oktaEnabled: boolean;
};

export const NewUsersPage = ({
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
      <Table>
        <Table.Head>
          <Table.Row>
            <Table.Cell>Email</Table.Cell>
            <Table.Cell>Roles</Table.Cell>
            {canSeeAdminControls && (
              <>
                <Table.Cell>Manage</Table.Cell>
                <Table.Cell>Remove</Table.Cell>
              </>
            )}
          </Table.Row>
        </Table.Head>
        <Table.Body>
          {users.map(user => {
            return (
              <Table.Row key={user.id}>
                <Table.Cell>{user.id}</Table.Cell>
                <Table.Cell>
                  {user.roleAssignments
                    .filter(ra => isVisibleRole(ra.role))
                    ?.map(ra => getRoleName(ra.role))
                    ?.join(' | ')}
                </Table.Cell>
                {canSeeAdminControls && (
                  <>
                    <Table.Cell>
                      {userProfile.emailAddress === user.id ||
                      !canEditUser(user.id) ? (
                        ''
                      ) : (
                        <EditRoleAssignmentsButton user={user.id} />
                      )}
                    </Table.Cell>
                    <Table.Cell>
                      {userProfile.emailAddress === user.id ||
                      !canEditUser(user.id) ? (
                        ''
                      ) : (
                        <RemoveUserButton user={user.id} />
                      )}
                    </Table.Cell>
                  </>
                )}
              </Table.Row>
            );
          })}
        </Table.Body>
      </Table>
    </>
  );
};

const AddRoleAssignmentsButton = () => (
  <div style={{ fontSize: 'large' }}>
    <Button href="users/manage">Add user</Button>
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
