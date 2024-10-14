﻿import NhsPage from '@components/nhs-page';
import { ManageUsersPage } from './manage-users-page';
import {
  fetchPermissions,
  fetchSite,
  fetchUserProfile,
} from '@services/appointmentsService';

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    user?: string;
  };
};

const AssignRolesPage = async ({ params, searchParams }: UserPageProps) => {
  const userProfile = await fetchUserProfile();
  const userIsSpecified = () =>
    (searchParams && 'user' in searchParams) ?? false;

  const site = await fetchSite(params.site);

  const permissions = await fetchPermissions(params.site);
  if (!permissions.includes('users:manage')) {
    throw new Error('Forbidden: You lack the necessary permissions');
  }

  return (
    <NhsPage
      title="Staff Role Management"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
      userProfile={userProfile}
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
