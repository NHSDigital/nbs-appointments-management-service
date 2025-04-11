import NhsPage from '@components/nhs-page';
import { ManageUsersPage } from './manage-users-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { notAuthorized } from '@services/authService';

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
  const oktaEnabledFlag = await fetchFeatureFlag('OktaEnabled');

  const userIsSpecified = () =>
    (searchParams && 'user' in searchParams) ?? false;
  const [site, userProfile] = await Promise.all([
    fetchSite(params.site),
    fetchUserProfile(),
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
      <ManageUsersPage
        userIsSpecified={userIsSpecified()}
        params={params}
        searchParams={searchParams}
        oktaEnabled={oktaEnabledFlag.enabled}
      />
    </NhsPage>
  );
};

export default AssignRolesPage;
