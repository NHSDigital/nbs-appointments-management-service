import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchRoles,
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

  const [site, userProfile, roleOptions] = await Promise.all([
    fetchSite(params.site),
    fetchUserProfile(),
    fetchRoles(),
  ]);

  if (userProfile.emailAddress === email) {
    notAuthorized();
  }

  return (
    <NhsPage
      title=""
      breadcrumbs={[
        { name: 'Home', href: '/sites' },
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
      originPage="users-manage"
    >
      <SetUserRolesWizard
        site={site}
        roleOptions={roleOptions}
        sessionUser={userProfile}
      />
    </NhsPage>
  );
};

export default AssignRolesPage;
