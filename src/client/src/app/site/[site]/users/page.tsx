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
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await assertAnyPermissions(siteFromPath, ['users:view', 'users:view']);
  const [userProfile, users, rolesResponse, site, permissions] =
    await Promise.all([
      fetchUserProfile(),
      fetchUsers(siteFromPath),
      fetchRoles(),
      fetchSite(siteFromPath),
      fetchPermissions(siteFromPath),
    ]);

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
