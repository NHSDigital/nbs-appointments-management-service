import {
  Breadcrumbs,
  Breadcrumb,
  WarningCallout,
} from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import { fetchUserProfile } from '@services/appointmentsService';

type Props = {
  title: string;
  children: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumb?: boolean;
};

const NhsPage = async ({
  title,
  children = null,
  breadcrumbs,
  omitTitleFromBreadcrumb,
}: Props) => {
  const userProfile = await fetchUserProfile();
  const breadcrumbsWithTitle = [...(breadcrumbs ?? [])];
  if (!omitTitleFromBreadcrumb) breadcrumbsWithTitle.push({ name: title });
  return (
    <>
      <NhsHeader userEmail={userProfile?.emailAddress} />
      <Breadcrumbs trail={breadcrumbsWithTitle} />
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
                    <WarningCallout title="You cannot access this site">
                      <p>
                        You are currently not signed in. To use this site,
                        please sign in.
                      </p>
                    </WarningCallout>
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
