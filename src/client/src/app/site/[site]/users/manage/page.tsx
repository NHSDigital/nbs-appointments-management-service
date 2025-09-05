import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchRoles,
  fetchUsers,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { notAuthorized } from '@services/authService';
import SetUserRolesWizard from './set-user-roles-wizard';
import NhsTransactionalPage from '@components/nhs-transactional-page';

export type UserPageProps = {
  searchParams?: Promise<{
    user?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const AssignRolesPage = async ({ params, searchParams }: UserPageProps) => {
  const { site: siteFromPath } = { ...(await params) };
  const { user: userFromParams } = { ...(await searchParams) };

  await assertPermission(siteFromPath, 'users:manage');

  const email = userFromParams?.toLowerCase();

  const [site, userProfile, roleOptions, userToEdit, oktaEnabled] =
    await Promise.all([
      fetchSite(siteFromPath),
      fetchUserProfile(),
      fetchRoles(),
      fetchUsers(siteFromPath).then(users => users.find(u => u.id === email)),
      fetchFeatureFlag('OktaEnabled'),
    ]);

  if (userProfile.emailAddress === email) {
    notAuthorized();
  }

  return (
    <NhsTransactionalPage title="" originPage="users-manage">
      <SetUserRolesWizard
        site={site}
        roleOptions={roleOptions}
        sessionUser={userProfile}
        userToEdit={userToEdit}
        oktaEnabled={oktaEnabled.enabled}
      />
    </NhsTransactionalPage>
  );
};

export default AssignRolesPage;
