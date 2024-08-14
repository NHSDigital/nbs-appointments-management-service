import Breadcrumbs, { Breadcrumb } from '@components/nhs-breadcrumbs';
import { ReactNode } from 'react';

type Props = {
  title: string;
  children: ReactNode;
  breadcrumbs?: Breadcrumb[];
};

const NhsPage = async ({ title, children = null, breadcrumbs }: Props) => {
  return (
    <>
      <Breadcrumbs breadcrumbs={breadcrumbs} />
      <div className="nhsuk-width-container app-width-container">
        <main
          className="govuk-main-wrapper app-main-wrapper"
          id="main-content"
          role="main"
        >
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-full app-component-reading-width">
              <div className="app-pane">
                <div className="app-pane__main-content">
                  <h1 className="app-page-heading">{title}</h1>
                  {children}
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </>
  );
};

export default NhsPage;
