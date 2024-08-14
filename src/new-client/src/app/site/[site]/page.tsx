import NhsNavCard from '@components/nhs-nav-card';
import NhsPage from '@components/nhs-page';
import { fetchUserProfile } from '@services/appointmentsService';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const userProfile = await fetchUserProfile();

  if (userProfile === undefined) return null;

  const site = userProfile.availableSites.find(s => s.id === params.site);

  if (site === undefined) throw Error('Cannot find information for site');

  return (
    <NhsPage title={site.name} breadcrumbs={[{ name: site.name }]}>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      <ul className="nhsuk-grid-row nhsuk-card-group">
        <li className="nhsuk-grid-column-two-thirds nhsuk-card-group__item">
          <NhsNavCard
            href={`${params.site}/users`}
            title="User Management"
            description="Assign roles to users to give them access to features at this site"
          />
        </li>
      </ul>
    </NhsPage>
  );
};

export default Page;
