import { fetchUserProfile } from '../../lib/auth';

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
    <div className="nhsuk-card">
      <div className="nhsuk-card__content">
        <h3 className="nhsuk-card__heading">{site.name}</h3>
        <p className="nhsuk-card__description">{site.address}</p>
      </div>
    </div>
  );
};

export default Page;
