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
  );
};
