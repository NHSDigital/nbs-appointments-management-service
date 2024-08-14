import {
  fetchUsers,
  fetchRoles,
  fetchSite,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { Table } from '@components/table';
import Link from 'next/link';
import { Role, User } from '@types';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const users = (await fetchUsers(params.site)) ?? [];
  const rolesResponse = await fetchRoles();
  const site = await fetchSite(params.site);

  return (
    <NhsPage
      title="Manage Staff Roles"
      breadcrumbs={[
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users' },
      ]}
    >
      <UsersPage users={users} roles={rolesResponse?.roles ?? []} />
    </NhsPage>
  );
};

interface Props {
  users: User[];
  roles: Role[];
}

export const UsersPage = ({ users, roles }: Props) => {
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

export default Page;
