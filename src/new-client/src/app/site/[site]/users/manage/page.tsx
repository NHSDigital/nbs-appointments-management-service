import NhsPage from '@components/nhs-page';
import { ManageUsersPage } from './manage-users-page';
import { fetchPermissions, fetchSite } from '@services/appointmentsService';

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
  const siteMoniker = site?.name ?? `Site ${params.site}`;

  const permissions = await fetchPermissions(params.site);
  if (!permissions.includes('users:manage')) {
    throw new Error('Forbidden: You lack the necessary permissions');
  }

  return (
    <NhsPage
      title="Staff Role Management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: siteMoniker, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
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
