import React from 'react';
import { ReactNode } from 'react';

type Props = {
  children: ReactNode;
};

/**
 * An Inset Text component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/inset-text
 */
const InsetText = ({ children }: Props) => {
  return (
    <div className="nhsuk-inset-text">
      <span className="nhsuk-u-visually-hidden">Information: </span>

      {children}
    </div>
  );
};

export default InsetText;
