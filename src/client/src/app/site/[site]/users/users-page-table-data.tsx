'use client';
import { Role, User } from '@types';
import { Table } from 'nhsuk-react-components';
import Link from 'next/link';

type DataProps = {
  users: User[];
  canSeeAdminControls?: boolean;
  userProfileEmail?: string;
  roles: Role[];
};

export const UsersPageTableData = ({
  users,
  canSeeAdminControls,
  userProfileEmail,
  roles,
}: DataProps) => {
  const isVisibleRole = (role: string) =>
    roles.find(r => r.id === role) !== undefined;
  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;

  return (
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
        {users.map(user => (
          <Table.Row key={user.id}>
            <Table.Cell>{user.id}</Table.Cell>
            <Table.Cell>
              {user.roleAssignments
                .filter(ra => isVisibleRole(ra.role))
                .map(ra => getRoleName(ra.role))
                .join(', ')}
            </Table.Cell>
            {canSeeAdminControls &&
              (userProfileEmail === user.id ? (
                <>
                  <Table.Cell></Table.Cell>
                  <Table.Cell></Table.Cell>
                </>
              ) : (
                <>
                  <Table.Cell>
                    <Link href={`users/manage?user=${user.id}`}>Edit</Link>
                  </Table.Cell>
                  <Table.Cell>
                    <Link href={`users/remove?user=${user.id}`}>
                      Remove from this site
                    </Link>
                  </Table.Cell>
                </>
              ))}
          </Table.Row>
        ))}
      </Table.Body>
    </Table>
  );
};
