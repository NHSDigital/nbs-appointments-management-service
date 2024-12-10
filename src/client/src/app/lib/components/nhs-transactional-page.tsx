import { ReactNode } from 'react';
import NhsMainContainer from './nhs-main-container';
import { Footer, Header } from '@nhsuk-frontend-components';
import NhsHeaderLogOut from './nhs-header-log-out';

type Props = {
  children: ReactNode;
};

const NhsTransactionalPage = ({ children = null }: Props) => {
  return (
    <>
      <Header showChangeSiteButton={false}>
        <NhsHeaderLogOut />
      </Header>
      <NhsMainContainer>{children}</NhsMainContainer>
      <Footer />
    </>
  );
};

export default NhsTransactionalPage;
