import NhsPage from '@components/nhs-page';
import RemoveUserPage from './remove-user-page';
import { fetchPermissions, fetchSite } from '@services/appointmentsService';
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
  // TODO: Should we throw these from the service itself? What side effects does have on other calls?
  if (searchParams?.user === undefined) {
    notFound();
  }

  const site = await fetchSite(params.site);
  if (site === undefined) {
    notFound();
  }

  const siteMoniker = site?.name ?? `Site ${params.site}`;

  const permissions = await fetchPermissions(params.site);
  if (!permissions.includes('users:manage')) {
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
