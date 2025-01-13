import { ReactNode } from 'react';
import NhsMainContainer from './nhs-main-container';
import { Footer, Header } from '@nhsuk-frontend-components';
import NhsHeaderLogOut from './nhs-header-log-out';
import FeedbackBanner from '@components/feedback-banner';

type Props = {
  children: ReactNode;
  originPage: string;
};

const NhsTransactionalPage = ({ children = null, originPage }: Props) => {
  return (
    <>
      <Header showChangeSiteButton={false}>
        <NhsHeaderLogOut />
      </Header>
      <FeedbackBanner originPage={originPage} />
      <NhsMainContainer>{children}</NhsMainContainer>
      <Footer />
    </>
  );
};

export default NhsTransactionalPage;
