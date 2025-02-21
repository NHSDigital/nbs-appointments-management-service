import { ReactNode } from 'react';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import { BackLink, Breadcrumbs, Header } from '@components/nhsuk-frontend';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';
import FeedbackBanner from '@components/feedback-banner';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  showHomeBreadcrumb?: boolean;
  originPage: string;
  showGoBack?: boolean;
} & NhsHeadingProps;

/**
 * A slimmed down version of NhsPage which can be loaded from client components.
 * It must not invoke server actions nor access http only cookies.
 */
const NhsAnonymousPage = ({
  title,
  caption,
  originPage,
  children = null,
  headerAuthComponent = null,
  showHomeBreadcrumb = false,
  showGoBack = false,
}: Props) => {
  return (
    <>
      <Header showChangeSiteButton={false}>{headerAuthComponent}</Header>
      <FeedbackBanner originPage={originPage} />
      {showHomeBreadcrumb && (
        <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      )}
      <NhsMainContainer>
        {showGoBack && (
          <BackLink renderingStrategy="server" text="Go back" href={'/'} />
        )}
        <NhsHeading title={title} caption={caption} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsAnonymousPage;
