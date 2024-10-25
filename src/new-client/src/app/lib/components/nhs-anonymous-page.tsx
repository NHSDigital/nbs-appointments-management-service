import { ReactNode } from 'react';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import { Breadcrumbs, Header } from '@components/nhsuk-frontend';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  showHomeBreadcrumb?: boolean;
} & NhsHeadingProps;

/**
 * A slimmed down version of NhsPage which can be loaded from client components.
 * It must not invoke server actions nor access http only cookies.
 */
const NhsAnonymousPage = ({
  title,
  caption,
  children = null,
  headerAuthComponent = null,
  showHomeBreadcrumb = false,
}: Props) => {
  return (
    <>
      <Header>{headerAuthComponent}</Header>
      {showHomeBreadcrumb && (
        <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      )}
      <NhsMainContainer>
        <NhsHeading title={title} caption={caption} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsAnonymousPage;
