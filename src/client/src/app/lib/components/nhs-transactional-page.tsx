import { ReactNode } from 'react';
import NhsMainContainer from './nhs-main-container';
import FeedbackBanner from '@components/feedback-banner';
import BackLink, { BackLinkProps } from '@components/nhsuk-frontend/back-link';
import NhsHeading from './nhs-heading';
import { cookies } from 'next/headers';
import NotificationBanner from './notification-banner';
import NhsFooter from './nhs-footer';
import NhsPageHeader from './nhsuk-frontend/nhs-page-header';
import { fetchUserProfile } from '@services/appointmentsService';
import fromServer from '@server/fromServer';

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

  const userProfile = await fromServer(fetchUserProfile());

  return (
    <>
      <NhsPageHeader
        showChangeSiteButton={false}
        userEmail={userProfile.emailAddress}
      />
      <FeedbackBanner originPage={originPage} />
      <NhsMainContainer>
        {backLink && <BackLink {...backLink} />}
        {title && <NhsHeading title={title} caption={caption} />}
        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsTransactionalPage;
