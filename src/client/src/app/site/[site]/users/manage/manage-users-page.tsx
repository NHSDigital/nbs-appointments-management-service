import UserDetails from './user-details';
import FindUserForm from './find-user-form';
import { When } from '@components/when';
import { UserPageProps } from './page';

export const ManageUsersPage = ({
  params,
  searchParams,
  userIsSpecified,
  oktaEnabled,
}: UserPageProps & { userIsSpecified: boolean; oktaEnabled: boolean }) => {
  return (
    <>
      <When condition={!userIsSpecified}>
        <h2>Add a user</h2>
        <>
          Email address must be nhs.net or on the list of{' '}
          <a href="https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/care-identity-email-domain-allow-list">
            authorised email domains.
          </a>{' '}
          Read the{' '}
          <a href="https://digital.nhs.uk/services/vaccinations-national-booking-service/manage-your-appointments-guidance/log-in-and-select-site">
            user guidance on logging in without an NHS.net account
          </a>{' '}
          or you can apply for their{' '}
          <a href="https://digital.nhs.uk/services/care-identity-service/applications-and-services/apply-for-care-id/request-an-addition-to-the-email-domain-allow-list">
            email domain to be approved.
          </a>
        </>
      </When>
      <div className="nhsuk-grid-row">
        <div className="nhsuk-grid-column-one-half">
          <When condition={userIsSpecified}>
            <UserDetails params={params} searchParams={searchParams} />
          </When>
          <When condition={!userIsSpecified}>
            <FindUserForm site={params.site} oktaEnabled={oktaEnabled} />
          </When>
        </div>
      </div>
    </>
  );
};
