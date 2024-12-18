import { forwardRef, HTMLProps } from 'react';

type Props = {
  label: string;
  hint?: string;
} & HTMLProps<HTMLInputElement>;
type Ref = HTMLInputElement;

/**
 * A radio input component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/radios
 */
export const Radio = forwardRef<Ref, Props>(
  ({ id, label, hint, value, ...props }, ref) => (
    <>
      <input
        className="nhsuk-radios__input"
        type="radio"
        value={value}
        ref={ref}
        id={id}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...props}
      />
      <label className="nhsuk-label nhsuk-radios__label" htmlFor={id}>
        {label}
      </label>
      {hint && (
        <div className="nhsuk-hint nhsuk-radios__hint" id={`${id}-item-hint`}>
          {hint}
        </div>
      )}
    </>
  ),
);
Radio.displayName = 'Radio';

export default Radio;
