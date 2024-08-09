import { fetchUserProfile } from '@services/nbsService';
import SiteList from '@components/site-list';

const Home = async () => {
  const userProfile = await fetchUserProfile();
  if (userProfile === undefined) {
    return null;
  }
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
