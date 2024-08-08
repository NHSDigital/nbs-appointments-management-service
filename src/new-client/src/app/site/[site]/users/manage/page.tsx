import NhsPageTitle from '@components/nhs-page-title';
import FindUserForm from './find-user-form';
import { When } from '@components/when';
import AssignRoles from './assign-roles';

export type UserPageProps = {
  params: {
    site: string;
  };
  searchParams?: {
    user?: string;
  };
};

const AssignRolesPage = ({ params, searchParams }: UserPageProps) => {
  const userIsSpecified = () =>
    (searchParams && 'user' in searchParams) ?? false;

  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <NhsPageTitle title="Staff Role Management" />
          <div className="nhsuk-hint">
            Set the details and roles of a new user
          </div>
        </div>

        <When condition={userIsSpecified()}>
          <AssignRoles params={params} searchParams={searchParams} />
        </When>
        <When condition={!userIsSpecified()}>
          <FindUserForm site={params.site} />
        </When>
      </div>
    </div>
  );
};

export default AssignRolesPage;
