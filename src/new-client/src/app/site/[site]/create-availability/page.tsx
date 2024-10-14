import NhsPage from '@components/nhs-page';
import { fetchSite, fetchUserProfile } from '@services/appointmentsService';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);
  const userProfile = await fetchUserProfile();

  return (
    <NhsPage
      title="Create Availability"
      breadcrumbs={[
        { name: 'Home', href: '/' },
        { name: site.name, href: `/site/${params.site}` },
      ]}
      userProfile={userProfile}
    >
      {/* This will be covered in APPT-240 */}
      <span></span>
    </NhsPage>
  );
};

export default Page;
