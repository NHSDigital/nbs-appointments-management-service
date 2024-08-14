'use client';
import Breadcrumbs from '@components/nhs-breadcrumbs';
import { NhsHeader } from '@components/nhs-header';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <>
      <NhsHeader showAuthControls={false} />
      <Breadcrumbs />
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
                  <h1 className="app-page-heading">An error has occurred</h1>
                  <ul
                    className="nhsuk-grid-row nhsuk-card-group"
                    style={{ padding: '20px' }}
                  >
                    <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
                      <div
                        className="nhsuk-error-summary"
                        aria-labelledby="error-summary-title"
                        role="alert"
                        tabIndex={-1}
                      >
                        <h2
                          className="nhsuk-error-summary__title"
                          id="error-summary-title"
                        >
                          There is a problem
                        </h2>
                        <div className="nhsuk-error-summary__body">
                          <p>{error.message}</p>
                        </div>
                      </div>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </>
  );
}
