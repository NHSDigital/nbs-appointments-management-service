type ErrorWithLink = {
  message: string;
  link: string;
};

export type ErrorSummaryProps = {
  message: string;
  errorsWithLinks?: ErrorWithLink[];
};

const ErrorSummaryCard = ({
  message,
  errorsWithLinks = [],
}: ErrorSummaryProps) => {
  return (
    <div className="nhsuk-card">
      <div
        className="nhsuk-error-summary"
        style={{ marginBottom: 0 }}
        aria-labelledby="error-summary-title"
        role="alert"
      >
        <h2 className="nhsuk-error-summary__title" id="error-summary-title">
          There is a problem
        </h2>
        <div className="nhsuk-error-summary__body">
          <p>{message}</p>
          {errorsWithLinks.length > 0 && (
            <ul className="nhsuk-list nhsuk-error-summary__list">
              {errorsWithLinks.map((error, index) => {
                return (
                  <li key={index}>
                    <a href={error.link}>{error.message}</a>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
};

export default ErrorSummaryCard;
