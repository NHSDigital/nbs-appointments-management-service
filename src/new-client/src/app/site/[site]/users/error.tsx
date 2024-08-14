'use client';
import NhsPage from '@components/nhs-page';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <NhsPage title="Error">
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
            <h2 className="nhsuk-error-summary__title" id="error-summary-title">
              There is a problem
            </h2>
            <div className="nhsuk-error-summary__body">
              <p>{error.message}</p>
            </div>
          </div>
        </li>
      </ul>
    </NhsPage>
  );
}
