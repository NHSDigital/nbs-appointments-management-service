import { forwardRef, HTMLProps } from 'react';

type Props = {
  label: string;
} & HTMLProps<HTMLInputElement>;
type Ref = HTMLInputElement;

/**
 * A text input component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/text-input
 */
export const TextInput = forwardRef<Ref, Props>((props, ref) => (
  <>
    <label className="nhsuk-label" htmlFor={props.id}>
      {props.label}
    </label>
    <input
      className="nhsuk-input"
      type="text"
      ref={ref}
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
    />
  </>
));
TextInput.displayName = 'TextInput';

export default TextInput;
