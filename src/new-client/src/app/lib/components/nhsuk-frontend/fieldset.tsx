import { ReactNode } from 'react';

type Props = {
  legend: string;
  children: ReactNode;
};

/**
 * A fieldset component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/fieldset
 */
const FieldSet = ({ legend, children }: Props) => {
  return (
    <fieldset className="nhsuk-fieldset">
      <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--s">
        <h1 className="nhsuk-fieldset__heading">{legend}</h1>
      </legend>

      {children}
    </fieldset>
  );
};

export default FieldSet;
