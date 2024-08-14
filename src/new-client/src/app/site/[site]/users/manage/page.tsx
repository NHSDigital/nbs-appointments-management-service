import NhsPage from '@components/nhs-page';
import NhsPageTitle from '@components/nhs-page-title';
import { When } from '@components/when';
import AssignRoles from './assign-roles';
import FindUserForm from './find-user-form';

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
    <NhsPage
      title="Staff Role Management"
      breadcrumbs={[
        { name: 'Site', href: `/site/${params.site}` },
        { name: 'Users' },
      ]}
    >
      <ManageUsersPage
        userIsSpecified={userIsSpecified()}
        params={params}
        searchParams={searchParams}
      />
    </NhsPage>
  );
};

export const ManageUsersPage = ({
  params,
  searchParams,
  userIsSpecified,
}: UserPageProps & { userIsSpecified: boolean }) => {
  return (
    <div className="nhsuk-grid-row">
      <div className="nhsuk-grid-column-one-half">
        <div className="nhsuk-form-group">
          <NhsPageTitle title="Staff Role Management" />
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

export default AssignRolesPage;
