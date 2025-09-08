import { ReactNode } from 'react';
import NhsMainContainer from './nhs-main-container';
import { Footer, Header } from '@nhsuk-frontend-components';
import NhsHeaderLogOut from './nhs-header-log-out';
import FeedbackBanner from '@components/feedback-banner';
import BackLink, { BackLinkProps } from '@components/nhsuk-frontend/back-link';
import NhsHeading from './nhs-heading';

type Props = {
  children: ReactNode;
  originPage: string;
  title?: string;
  caption?: string;
  backLink?: BackLinkProps;
};

const NhsTransactionalPage = ({
  children = null,
  originPage,
  title,
  caption,
  backLink,
}: Props) => {
  return (
    <>
      <Header showChangeSiteButton={false}>
        <NhsHeaderLogOut />
      </Header>
      <FeedbackBanner originPage={originPage} />
      <NhsMainContainer>
        {backLink && <BackLink {...backLink} />}
        {title && <NhsHeading title={title} caption={caption} />}
        {children}
      </NhsMainContainer>
      <Footer />
    </>
  );
};

export default NhsTransactionalPage;
