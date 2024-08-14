import NhsPage from '@components/nhs-page';
import ManageUsersPage from './manage-users-page';

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
    <NhsPage title="Staff Role Management">
      <ManageUsersPage
        userIsSpecified={userIsSpecified()}
        params={params}
        searchParams={searchParams}
      />
    </NhsPage>
  );
};

export default AssignRolesPage;
