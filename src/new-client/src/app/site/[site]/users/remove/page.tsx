import NhsPage from '@components/nhs-page';
import RemoveUserPage from './remove-user-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
  fetchUsers,
} from '@services/appointmentsService';
import { notFound, redirect } from 'next/navigation';
import { notAuthorised } from '@services/authService';

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

  const site = await fetchSite(params.site);

  const users = await fetchUsers(params.site);
  if (users === undefined || !users.some(u => u.id === searchParams?.user)) {
    notFound();
  }

  await assertPermission(site.id, 'users:manage');

  const userProfile = await fetchUserProfile();
  if (userProfile.emailAddress === searchParams?.user) {
    notAuthorised();
  }

  return (
    <NhsPage
      title="Remove User"
      breadcrumbs={[
        { name: site.name, href: `/site/${params.site}` },
        { name: 'Users', href: `/site/${params.site}/users` },
      ]}
    >
      <RemoveUserPage user={searchParams?.user} site={site} />
    </NhsPage>
  );
};

export default Page;
