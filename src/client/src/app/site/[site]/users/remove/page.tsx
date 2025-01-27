import NhsPage from '@components/nhs-page';
import RemoveUserPage from './remove-user-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchUsers,
} from '@services/appointmentsService';
import { notFound, redirect } from 'next/navigation';
import { notAuthorized } from '@services/authService';

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    user?: string;
  };
};

const Page = async ({ params, searchParams }: UserPageProps) => {
  if (searchParams?.user === undefined) {
    redirect(`/site/${params.site}/users`);
  }

  const [site, users, userProfile] = await Promise.all([
    fetchSite(params.site),
    fetchUsers(params.site),
    fetchUserProfile(),
    assertPermission(params.site, 'users:manage'),
  ]);

  if (users === undefined || !users.some(u => u.id === searchParams?.user)) {
    notFound();
  }

  if (userProfile.emailAddress === searchParams?.user) {
    notAuthorized();
  }

  return (
    <NhsPage
      title="Remove User"
      breadcrumbs={[
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
      originPage="users-remove"
    >
      <RemoveUserPage user={searchParams?.user} site={site} />
    </NhsPage>
  );
};

export default Page;
