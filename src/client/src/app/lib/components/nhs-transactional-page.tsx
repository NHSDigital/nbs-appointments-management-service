import { ReactNode } from 'react';
import NhsMainContainer from './nhs-main-container';
import { Footer, Header } from '@nhsuk-frontend-components';
import NhsHeaderLogOut from './nhs-header-log-out';
import FeedbackBanner from '@components/feedback-banner';
import BackLink, { BackLinkProps } from '@components/nhsuk-frontend/back-link';
import NhsHeading from './nhs-heading';
import { cookies } from 'next/headers';
import NotificationBanner from './notification-banner';

type Props = {
  children: ReactNode;
  originPage: string;
  title?: string;
  caption?: string;
  backLink?: BackLinkProps;
};

const NhsTransactionalPage = async ({
  children = null,
  originPage,
  title,
  caption,
  backLink,
}: Props) => {
  const cookieStore = await cookies();
  const notification = cookieStore.get('ams-notification')?.value;

  return (
    <>
      <Header showChangeSiteButton={false}>
        <NhsHeaderLogOut />
      </Header>
      <FeedbackBanner originPage={originPage} />
      <NhsMainContainer>
        {backLink && <BackLink {...backLink} />}
        {title && <NhsHeading title={title} caption={caption} />}
        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <Footer />
    </>
  );
};

export default NhsTransactionalPage;
