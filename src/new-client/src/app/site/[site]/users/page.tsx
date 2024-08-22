import {
  fetchUsers,
  fetchRoles,
  fetchSite,
} from '@services/appointmentsService';
import NhsPage from '@components/nhs-page';
import { UsersPage } from './users-page';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const users = (await fetchUsers(params.site)) ?? [];
  const rolesResponse = await fetchRoles();
  const site = await fetchSite(params.site);
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  return (
    <NhsPage
      title="Manage Staff Roles"
      breadcrumbs={[{ name: siteMoniker, href: `/site/${params.site}` }]}
    >
      <UsersPage users={users} roles={rolesResponse?.roles ?? []} />
    </NhsPage>
  );
};

export default Page;
