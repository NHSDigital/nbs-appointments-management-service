import { Header } from 'nhsuk-react-components';

const NhsAnonymousHeader = () => {
  return (
    <Header
      service={{
        href: '/manage-your-appointments/sites',
        text: 'Manage Your Appointments',
      }}
    ></Header>
  );
};

export default NhsAnonymousHeader;
