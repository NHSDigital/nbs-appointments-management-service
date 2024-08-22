import { ReactNode } from 'react';

type Props = {
  children: ReactNode;
  prompt?: {
    legend: string;
    hint: string;
    id: string;
  };
};

/**
 * A checkbox component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/checkboxes
 */
const CheckBoxes = ({ children, prompt }: Props) => {
  return (
    <div className="nhsuk-form-group">
      <fieldset className="nhsuk-fieldset" aria-describedby={prompt?.id}>
        {prompt && (
          <>
            <legend className="nhsuk-fieldset__legend nhsuk-fieldset__legend--1">
              <h1 className="nhsuk-fieldset__heading">{prompt.legend}</h1>
            </legend>

            <div className="nhsuk-hint" id={prompt.id}>
              {prompt.hint}
            </div>
          </>
        )}

        <div className="nhsuk-checkboxes">{children}</div>
      </fieldset>
    </div>
  );
};

export default CheckBoxes;
