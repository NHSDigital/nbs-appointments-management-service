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
import fromServer from '@server/fromServer';

type PageProps = {
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params }: PageProps) => {
  const { site: siteFromPath } = { ...(await params) };

  await fromServer(
    assertAnyPermissions(siteFromPath, ['users:view', 'users:view']),
  );

  const [userProfile, users, rolesResponse, site, permissions] =
    await Promise.all([
      fromServer(fetchUserProfile()),
      fromServer(fetchUsers(siteFromPath)),
      fromServer(fetchRoles()),
      fromServer(fetchSite(siteFromPath)),
      fromServer(fetchPermissions(siteFromPath)),
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
