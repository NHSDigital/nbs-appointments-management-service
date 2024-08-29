import { ReactNode } from 'react';

type Props = {
  children: ReactNode;
};

/**
 * A checkbox component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/checkboxes
 */
const CheckBoxes = ({ children }: Props) => {
  return <div className="nhsuk-checkboxes">{children}</div>;
};

export default CheckBoxes;
