import { ReactNode } from 'react';

type Props = {
  title?: string;
  children: ReactNode;
};

/**
 * A warning callout component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/warning-callout
 */
const WarningCallout = ({ title = 'Important', children }: Props) => {
  return (
    <div className="nhsuk-warning-callout">
      <h3 className="nhsuk-warning-callout__label">
        <span>
          {!title.includes('Important') ? (
            <span className="nhsuk-u-visually-hidden">Important: </span>
          ) : null}

          {title}
        </span>
      </h3>
      <p>{children}</p>
    </div>
  );
};

export default WarningCallout;
