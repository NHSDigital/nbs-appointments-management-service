import NhsPageTitle from '@components/nhs-page-title';
import AssignRoles from './assign-roles';
import FindUserForm from './find-user-form';
import { When } from '@components/when';
import { UserPageProps } from './page';

export const ManageUsersPage = ({
  params,
  searchParams,
  userIsSpecified,
}: UserPageProps & { userIsSpecified: boolean }) => {
  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <div className="nhsuk-hint">
            Set the details and roles of a new user
          </div>
        </div>

        <When condition={userIsSpecified}>
          <AssignRoles params={params} searchParams={searchParams} />
        </When>
        <When condition={!userIsSpecified}>
          <FindUserForm site={params.site} />
        </When>
      </div>
    </div>
  );
};
