import NhsPage from '@components/nhs-page';
import { ManageUsersPage } from './manage-users-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
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
  const userIsSpecified = () =>
    (searchParams && 'user' in searchParams) ?? false;

  const site = await fetchSite(params.site);

  await assertPermission(site.id, 'users:manage');

  const userProfile = await fetchUserProfile();
  if (userProfile.emailAddress === searchParams?.user) {
    notAuthorized();
  }

  return (
    <NhsPage
      title="Staff Role Management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
      originPage="users-manage"
    >
      <ManageUsersPage
        userIsSpecified={userIsSpecified()}
        params={params}
        searchParams={searchParams}
      />
    </NhsPage>
  );
};

export default AssignRolesPage;
