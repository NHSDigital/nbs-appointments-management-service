import { fetchUserProfile } from '../../lib/auth';
import NhsNavCard from '@components/nhs-nav-card';

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const userProfile = await fetchUserProfile();

  if (userProfile === undefined) throw Error('failed to retrieve user profile');

  const site = userProfile.availableSites.find(s => s.id === params.site);

  if (site === undefined) throw Error('Cannot find information for site');

  return (
    <div>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      <ul
        className="nhsuk-grid-row nhsuk-card-group"
        style={{ padding: '20px' }}
      >
        <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
          <NhsNavCard
            href={`${params.site}/availability/month`}
            title="Availability"
            description="Configure availability and open appointments for your site"
          />
        </li>
      </ul>
    </div>
  );
};

export default Page;
