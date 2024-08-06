import { Table } from '@components/table';
import { fetchUsers } from '../../../lib/users';
import { fetchRoles } from '../../../lib/roles';
import NhsPageTitle from '@components/nhs-page-title';

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

export default UsersPage;
