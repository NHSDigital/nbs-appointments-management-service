import {
  fetchUsers,
  fetchRoles,
  fetchSite,
  fetchPermissions,
  fetchUserProfile,
  assertAnyPermissions,
  fetchFeatureFlag,
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

  const [userProfile, users, rolesResponse, site, permissions, oktaEnabled] =
    await Promise.all([
      fetchUserProfile(),
      fetchUsers(siteFromPath),
      fetchRoles(),
      fetchSite(siteFromPath),
      fetchPermissions(siteFromPath),
      fetchFeatureFlag('OktaEnabled'),
    ]);

  return (
    <NhsPage title="Manage users" site={site} originPage="users">
      <UsersPage
        userProfile={userProfile}
        users={users}
        roles={rolesResponse}
        permissions={permissions}
        oktaEnabled={oktaEnabled.enabled}
      />
    </NhsPage>
  );
};

export default Page;
