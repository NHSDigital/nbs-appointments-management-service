import NhsPage from '@components/nhs-page';
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

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams: {
    user?: string;
  };
};

const AssignRolesPage = async ({ params, searchParams }: UserPageProps) => {
  await assertPermission(params.site, 'users:manage');

  const email = searchParams.user?.toLowerCase();

  const [site, userProfile, roleOptions, userToEdit, oktaEnabled] =
    await Promise.all([
      fetchSite(params.site),
      fetchUserProfile(),
      fetchRoles(),
      fetchUsers(params.site).then(users => users.find(u => u.id === email)),
      fetchFeatureFlag('OktaEnabled'),
    ]);

  if (userProfile.emailAddress === email) {
    notAuthorized();
  }

  return (
    <NhsPage title="" originPage="users-manage">
      <SetUserRolesWizard
        site={site}
        roleOptions={roleOptions}
        sessionUser={userProfile}
        userToEdit={userToEdit}
        oktaEnabled={oktaEnabled.enabled}
      />
    </NhsPage>
  );
};

export default AssignRolesPage;
