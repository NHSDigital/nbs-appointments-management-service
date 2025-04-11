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
  await assertAnyPermissions(params.site, ['users:view', 'users:view']);
  const [userProfile, users, rolesResponse, site, permissions] =
    await Promise.all([
      fetchUserProfile(),
      fetchUsers(params.site),
      fetchRoles(),
      fetchSite(params.site),
      fetchPermissions(params.site),
    ]);

  return (
    <NhsPage title="Manage users" site={site} originPage="users">
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
