import AssignRoles from './assign-roles';
import FindUserForm from './find-user-form';
import { When } from '@components/when';
import { Site } from '@types';

type ManageUsersPageProps = {
  site: Site;
  user?: string;
};

export const ManageUsersPage = ({ site, user }: ManageUsersPageProps) => {
  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <div className="nhsuk-hint">
            Set the details and roles of a new user
          </div>
        </div>

        <When condition={user !== undefined}>
          <AssignRoles site={site.id} user={user} />
        </When>
        <When condition={user === undefined}>
          <FindUserForm site={site.id} />
        </When>
      </div>
    </div>
  );
};
