import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchRoles,
  fetchUsers,
} from '@services/appointmentsService';
import { notAuthorized } from '@services/authService';
import SetUserRolesWizard from './set-user-roles-wizard';
import NhsTransactionalPage from '@components/nhs-transactional-page';
import fromServer from '@server/fromServer';

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

  await fromServer(assertPermission(siteFromPath, 'users:manage'));

  const email = userFromParams?.toLowerCase();

  const [site, userProfile, roleOptions, userToEdit] = await Promise.all([
    fromServer(fetchSite(siteFromPath)),
    fromServer(fetchUserProfile()),
    fromServer(fetchRoles()),
    fromServer(fetchUsers(siteFromPath)).then(users =>
      users.find(u => u.id === email),
    ),
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
      />
    </NhsTransactionalPage>
  );
};

export default AssignRolesPage;
