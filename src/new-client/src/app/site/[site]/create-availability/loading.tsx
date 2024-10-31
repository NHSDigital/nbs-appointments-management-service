import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { Spinner } from '@components/nhsuk-frontend';

const LoadingPage = () => {
  return (
    <NhsAnonymousPage title="Appointment Management Service">
      <Spinner />
    </NhsAnonymousPage>
  );
};

export default LoadingPage;
