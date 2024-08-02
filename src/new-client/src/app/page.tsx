import SiteList from '@components/site-list';
import { fetchUserProfile } from './lib/auth';

const Home = async () => {
  const userProfile = await fetchUserProfile();

  if (userProfile === undefined) throw Error('failed to retrieve user profile');

  return (
    <div className="nhsuk-grid-row nhsuk-main-wrapper nhsuk-width-container">
      <div className="nhsuk-grid-column-full">
        <h1>Appointment Management Service</h1>
        <div>
          <SiteList sites={userProfile.availableSites} />
        </div>
      </div>
    </div>
  );
};

export default Home;
