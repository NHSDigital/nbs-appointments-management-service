import NhsPage from '@components/nhs-page';
import RemoveUserPage from './remove-user-page';
import {
  assertPermission,
  fetchSite,
  fetchUserProfile,
} from '@services/appointmentsService';
import { redirect } from 'next/navigation';

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

  await assertPermission(site.id, 'users:manage');

  const userProfile = await fetchUserProfile();
  if (userProfile.emailAddress === searchParams?.user) {
    throw new Error('Forbidden: You lack the necessary permissions');
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
