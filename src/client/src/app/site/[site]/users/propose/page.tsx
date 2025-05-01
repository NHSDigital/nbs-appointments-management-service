import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { notAuthorized } from '@services/authService';
import ProposeNewUserForm from './propose-new-user-form';

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    user?: string;
  };
};

const AssignRolesPage = async ({ params, searchParams }: UserPageProps) => {
  await assertPermission(params.site, 'users:manage');

  const [site, userProfile] = await Promise.all([
    fetchSite(params.site),
    fetchUserProfile(),
    fetchFeatureFlag('OktaEnabled'),
  ]);

  if (userProfile.emailAddress === searchParams?.user) {
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
      <ProposeNewUserForm site={params.site} />
    </NhsPage>
  );
};

export default AssignRolesPage;
