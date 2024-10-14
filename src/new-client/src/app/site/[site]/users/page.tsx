import {
  fetchUsers,
  fetchRoles,
  fetchSite,
  fetchPermissions,
  fetchUserProfile,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { UsersPage } from './users-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const userProfile = await fetchUserProfile();
  const users = await fetchUsers(params.site);
  const rolesResponse = await fetchRoles();
  const site = await fetchSite(params.site);
  const permissions = await fetchPermissions(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  // TODO: This check will be unnecessary after APPT-202 is merged
  if (userProfile === undefined) {
    throw new Error('Not logged in');
  }

  return (
    <NhsPage
      title="Manage Staff Roles"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
      ]}
    >
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
