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
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  await assertAnyPermissions(params.site, ['users:view', 'users:view']);
  const [userProfile, users, rolesResponse, site, permissions, oktaEnabled] =
    await Promise.all([
      fetchUserProfile(),
      fetchUsers(params.site),
      fetchRoles(),
      fetchSite(params.site),
      fetchPermissions(params.site),
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
