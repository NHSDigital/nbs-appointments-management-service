import { ReactNode } from 'react';

type Props = {
  summary: string | ReactNode;
  children: ReactNode;
};

/**
 * A details component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/details
 */
const Details = ({ summary, children }: Props) => {
  return (
    <details className="nhsuk-details">
      <summary className="nhsuk-details__summary">
        <span className="nhsuk-details__summary-text">{summary}</span>
      </summary>
      <div className="nhsuk-details__text">{children}</div>
    </details>
  );
};

export default Details;
