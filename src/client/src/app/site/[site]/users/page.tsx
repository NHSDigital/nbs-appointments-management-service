import {
  fetchUsers,
  fetchRoles,
  fetchSite,
  fetchPermissions,
  fetchUserProfile,
  assertAnyPermissions,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { UsersPage } from './users-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const userProfile = await fetchUserProfile();
  const users = await fetchUsers(params.site);
  const rolesResponse = await fetchRoles();
  const site = await fetchSite(params.site);
  const permissions = await fetchPermissions(params.site);

  await assertAnyPermissions(site.id, ['users:view', 'users:view']);

  return (
    <NhsPage title="Manage Staff Roles" site={site} originPage="users">
      <UsersPage
        userProfile={userProfile}
        users={users}
        roles={rolesResponse}
        permissions={permissions}
      />
    </NhsPage>
  );
};

export default Page;
