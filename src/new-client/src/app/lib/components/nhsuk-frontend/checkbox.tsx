import { forwardRef, HTMLProps } from 'react';

type Props = {
  label: string;
} & HTMLProps<HTMLInputElement>;
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
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
    />
    <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor={props.id}>
      {props.label}
    </label>
  </div>
));
CheckBox.displayName = 'CheckBox';

export default CheckBox;
