import NhsPage from '@components/nhs-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchUsers,
} from '@services/appointmentsService';
import { notFound } from 'next/navigation';
import { notAuthorized } from '@services/authService';
import RemoveUserPage from './remove-user-page';

export type UserPageProps = {
  params: {
    site: string;
    user: string;
  };
};

const Page = async ({ params }: UserPageProps) => {
  await assertPermission(params.site, 'users:manage');

  const email = decodeURIComponent(params.user).toLowerCase();

  const [site, users, userProfile] = await Promise.all([
    fetchSite(params.site),
    fetchUsers(params.site),
    fetchUserProfile(),
  ]);

  if (users === undefined || !users.some(u => u.id === email)) {
    notFound();
  }

  if (userProfile.emailAddress === email) {
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
      <RemoveUserPage user={params.user} site={site} />
    </NhsPage>
  );
};

export default Page;
