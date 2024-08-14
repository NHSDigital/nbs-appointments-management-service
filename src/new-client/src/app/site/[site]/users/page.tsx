import {
  fetchUsers,
  fetchRoles,
  fetchSite,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import UsersPage from './users-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const users = (await fetchUsers(params.site)) ?? [];
  const roles = (await fetchRoles()) ?? [];
  const site = await fetchSite(params.site);

  return (
    <NhsPage
      title="Manage Staff Roles"
      breadcrumbs={[{ name: 'Users', href: `/site/${site.id}/users` }]}
    >
      <UsersPage users={users} roles={roles} />
    </NhsPage>
  );
};

export default Page;
