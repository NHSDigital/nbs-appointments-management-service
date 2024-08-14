import Breadcrumbs, { Breadcrumb } from '@components/nhs-breadcrumbs';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import { fetchUserProfile } from '@services/appointmentsService';
import NhsWarning from './nhs-warning';

type Props = {
  title: string;
  children: ReactNode;
  breadcrumbs?: Breadcrumb[];
};

const NhsPage = async ({ title, children = null, breadcrumbs }: Props) => {
  const userProfile = await fetchUserProfile();

  return (
    <>
      <NhsHeader userEmail={userProfile?.emailAddress} />
      <Breadcrumbs trail={breadcrumbs} />
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
                  {userProfile === undefined ? (
                    <NhsWarning title="You cannot access this site">
                      <p>
                        You are currently not signed in. To use this site,
                        please sign in.
                      </p>
                    </NhsWarning>
                  ) : (
                    children
                  )}
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
