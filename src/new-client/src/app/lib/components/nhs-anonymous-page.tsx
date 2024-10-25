import { ReactNode } from 'react';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import { Breadcrumbs, Header } from '@components/nhsuk-frontend';

type Props = {
  title: string;
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  showHomeBreadcrumb?: boolean;
};

/**
 * A slimmed down version of NhsPage which can be loaded from client components.
 * It must not invoke server actions nor access http only cookies.
 */
const NhsAnonymousPage = ({
  title,
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
        <h1 className="app-page-heading">{title}</h1>
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

export default NhsAnonymousPage;
