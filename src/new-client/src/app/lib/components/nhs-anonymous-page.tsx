import { Breadcrumbs } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import NhsFooter from '@components/nhs-footer';

type Props = {
  title: string;
  children: ReactNode;
};

/**
 * A slimmed down version of NhsPage for use in pages which must not
 * make server calls.
 */
const NhsAnonymousPage = ({ title, children = null }: Props) => {
  return (
    <>
      <NhsHeader showAuthControls={false} />
      <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      <div className="nhsuk-width-container">
        <main className="nhsuk-main-wrapper" id="main-content" role="main">
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-full">
              <h1 className="app-page-heading">{title}</h1>
              {children}
            </div>
          </div>
        </main>
      </div>
      <NhsFooter />
    </>
  );
};

export default NhsAnonymousPage;
