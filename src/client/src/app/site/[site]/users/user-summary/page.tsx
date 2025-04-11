import NhsPage from '@components/nhs-page';
import UserSummary from './user-summary';

const Page = async () => {
  return (
    <NhsPage title="Check user details" originPage="user-summary">
      <UserSummary />
    </NhsPage>
  );
};

export default Page;
