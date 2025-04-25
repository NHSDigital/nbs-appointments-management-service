import NhsPage from '@components/nhs-page';
import UserSummary from './user-summary';
import { fetchRoles } from '@services/appointmentsService';

const Page = async () => {
  const roles = await fetchRoles();

  return (
    <NhsPage title="Check user details" originPage="user-summary">
      <UserSummary roles={roles} />
    </NhsPage>
  );
};

export default Page;
