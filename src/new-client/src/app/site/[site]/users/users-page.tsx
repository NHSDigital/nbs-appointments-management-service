import { Table } from '@components/table';
import { User, Role } from '@types';
import Link from 'next/link';

interface Props {
  users: User[];
  roles: Role[];
}

const UsersPage = ({ users, roles }: Props) => {
  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;

  return (
    <>
      <div style={{ display: 'flex', alignItems: 'baseline' }}>
        <span className="nhsuk-hint" style={{ flexGrow: '1' }}>
          Manage your current site's staff roles
        </span>
        <span>
          <AddRoleAssignmentsButton />
        </span>
      </div>
      <Table
        caption={`Manage your current site's staff roles`}
        headers={['Email', 'Roles']}
        rows={users.map(user => {
          return [
            user.id,
            user.roleAssignments?.map(ra => getRoleName(ra.role))?.join(' | '),
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

// const EditRoleAssignmentsButton = ({ user }: { user: string }) => (
//   <div>
//     <Link href={`users/manage?user=${user}`} className="nhsuk-link">
//       Edit
//     </Link>
//   </div>
// );

export default UsersPage;
