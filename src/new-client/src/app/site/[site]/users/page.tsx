import { Table } from '@components/table';
import NhsPageTitle from '@components/nhs-page-title';
import Link from 'next/link';
import { fetchUsers, fetchRoles } from '@services/nbsService';

type PageProps = {
  params: {
    site: string;
  };
};

const UsersPage = async ({ params }: PageProps) => {
  const users = await fetchUsers(params.site);
  const roles = await fetchRoles();
  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;
  return (
    <>
      <NhsPageTitle title="Manage Staff Roles" />
      <div>
        <div style={{ display: 'flex', alignItems: 'baseline' }}>
          <span className="nhsuk-hint" style={{ flexGrow: '1' }}>
            Manage your current site's staff roles
          </span>
          <span>
            <AddRoleAssignmentsButton />
          </span>
        </div>
        <Table
          headers={['Email', 'Roles', 'Manage']}
          rows={users.map(user => {
            return [
              user.id,
              user.roleAssignments
                ?.map(ra => getRoleName(ra.role))
                ?.join(' | '),
              <EditRoleAssignmentsButton key={user.id} user={user.id} />,
            ];
          })}
        />
      </div>
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

export default UsersPage;
