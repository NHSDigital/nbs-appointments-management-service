import Breadcrumbs, { Breadcrumb } from '@components/nhs-breadcrumbs';
import { NhsHeader } from './nhs-header';

type Props = {
  title: string;
  message: string;
  breadcrumbs?: Breadcrumb[];
};

const NhsErrorPage = ({ title, message, breadcrumbs }: Props) => {
  return (
    <>
      <NhsHeader showAuthControls={false} />
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
                          <p>{message}</p>
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
};

export default NhsErrorPage;
