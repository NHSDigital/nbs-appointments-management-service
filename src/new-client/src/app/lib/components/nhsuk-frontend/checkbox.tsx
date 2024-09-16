import { forwardRef } from 'react';

type Props = {
  label: string;
  hint?: string;
} & React.DetailedHTMLProps<
  React.InputHTMLAttributes<HTMLInputElement>,
  HTMLInputElement
>;
type Ref = HTMLInputElement;

/**
 * A checkbox component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/checkboxes
 */
export const CheckBox = forwardRef<Ref, Props>((props, ref) => (
  <div className="nhsuk-checkboxes__item">
    <input
      className="nhsuk-checkboxes__input"
      type="checkbox"
      ref={ref}
      aria-describedby={props.hint ? `${props.id}-item-hint` : undefined}
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
    />
    <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor={props.id}>
      {props.label}
    </label>
    <div
      className="nhsuk-hint nhsuk-checkboxes__hint"
      id={`${props.id}-item-hint`}
    >
      {props.hint}
    </div>
  </div>
));
CheckBox.displayName = 'CheckBox';

export default CheckBox;
