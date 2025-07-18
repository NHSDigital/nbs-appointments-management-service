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
  searchParams?: Promise<{
    user?: string;
  }>;
  params: Promise<{
    site: string;
  }>;
};

const Page = async ({ params, searchParams }: UserPageProps) => {
  const { user } = { ...(await searchParams) };
  const { site: siteFromPath } = { ...(await params) };

  if (user === undefined) {
    redirect(`/site/${siteFromPath}/users`);
  }

  await assertPermission(siteFromPath, 'users:manage');

  const [site, users, userProfile] = await Promise.all([
    fetchSite(siteFromPath),
    fetchUsers(siteFromPath),
    fetchUserProfile(),
  ]);

  if (users === undefined || !users.some(u => u.id === user.toLowerCase())) {
    notFound();
  }

  if (userProfile.emailAddress === user.toLowerCase()) {
    notAuthorized();
  }

  return (
    <NhsPage
      title="Remove User"
      breadcrumbs={[
        { name: site.name, href: `/site/${site.id}` },
        { name: 'Users', href: `/site/${site.id}/users` },
      ]}
      originPage="users-remove"
    >
      <RemoveUserPage user={user} site={site} />
    </NhsPage>
  );
};

export default Page;
