import NhsPage from '@components/nhs-page';
import RemoveUserPage from './remove-user-page';
import {
  fetchPermissions,
  fetchSite,
  fetchUserProfile,
  fetchUsers,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    user?: string;
  };
};

const Page = async ({ params, searchParams }: UserPageProps) => {
  // TODO: Clean up these checks after appt-202 is merged and site/users results can be relied upon
  if (searchParams?.user === undefined) {
    notFound();
  }

  const site = await fetchSite(params.site);
  if (site === undefined) {
    notFound();
  }

  const users = await fetchUsers(params.site);
  if (users === undefined || !users.some(u => u.id === searchParams?.user)) {
    notFound();
  }

  const siteMoniker = site?.name ?? `Site ${params.site}`;

  const permissions = await fetchPermissions(params.site);
  if (!permissions.includes('users:manage')) {
    throw new Error('Forbidden: You lack the necessary permissions');
  }

  const userProfile = await fetchUserProfile();
  if (
    userProfile === undefined ||
    userProfile.emailAddress === searchParams?.user
  ) {
    throw new Error('Forbidden: You lack the necessary permissions');
  }

  return (
    <NhsPage
      title="Remove User"
      breadcrumbs={[
        { name: siteMoniker, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
    >
      <RemoveUserPage user={searchParams?.user} site={site} />
    </NhsPage>
  );
};

export default Page;
