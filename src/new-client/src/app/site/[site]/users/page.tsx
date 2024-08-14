import { Table } from '@components/table';
import Link from 'next/link';
import {
  fetchUsers,
  fetchRoles,
  fetchUserProfile,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';

type PageProps = {
  params: {
    site: string;
  };
};

const UsersPage = async ({ params }: PageProps) => {
  const userProfile = await fetchUserProfile();
  const users = (await fetchUsers(params.site)) ?? [];
  const roles = (await fetchRoles()) ?? [];

  if (userProfile === undefined) return null;

  const site = userProfile.availableSites.find(s => s.id === params.site);
  if (site === undefined) throw Error('Cannot find information for site');

  const getRoleName = (role: string) =>
    roles.find(r => r.id === role)?.displayName;

  return (
    <NhsPage
      title="Manage Staff Roles"
      breadcrumbs={[{ name: 'Users', href: `/site/${site.id}/users` }]}
    >
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
    </NhsPage>
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
