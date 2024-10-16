import { forwardRef, HTMLProps } from 'react';

type Props = {
  label: string;
  inputType?: 'text' | 'date';
} & HTMLProps<HTMLInputElement>;
type Ref = HTMLInputElement;

/**
 * A text input component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/text-input
 */
export const TextInput = forwardRef<Ref, Props>(
  ({ id, label, inputType, ...props }, ref) => (
    <>
      <label className="nhsuk-label" htmlFor={id}>
        {label}
      </label>
      <input
        className={`nhsuk-input ${inputType === 'date' ? 'nhsuk-date-input__input nhsuk-input--width-2' : ''}`}
        ref={ref}
        inputMode={inputType === 'date' ? 'numeric' : 'text'}
        id={id}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...props}
      />
    </>
  ),
);
TextInput.displayName = 'TextInput';

export default TextInput;
