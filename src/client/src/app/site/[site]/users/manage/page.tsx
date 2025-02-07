import NhsPage from '@components/nhs-page';
import { ManageUsersPage } from './manage-users-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
} from '@services/appointmentsService';
import { notAuthorized } from '@services/authService';

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

  const [site, userProfile] = await Promise.all([
    fetchSite(siteFromPath),
    fetchUserProfile(),
  ]);

  if (userProfile.emailAddress === userFromParams) {
    notAuthorized();
  }

  return (
    <NhsPage
      title="Staff Role Management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${siteFromPath}` },
        { name: 'Users', href: `/site/${siteFromPath}/users` },
      ]}
      originPage="users-manage"
    >
      <ManageUsersPage user={userFromParams} site={site} />
    </NhsPage>
  );
};

export default AssignRolesPage;
