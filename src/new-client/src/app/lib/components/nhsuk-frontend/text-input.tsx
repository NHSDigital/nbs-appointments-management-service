import { forwardRef, HTMLProps } from 'react';

type Props = {
  label?: string;
  inputType?: 'text' | 'date';
  width?: 2 | 3 | 4 | 5 | 10 | 20;
  prefix?: string;
  suffix?: string;
} & HTMLProps<HTMLInputElement>;
type Ref = HTMLInputElement;

/**
 * A text input component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/text-input
 */
export const TextInput = forwardRef<Ref, Props>(
  ({ id, label, inputType, width, prefix, suffix, ...props }, ref) => {
    if (inputType === 'date') {
      return (
        <>
          {label && (
            <label className="nhsuk-label" htmlFor={id}>
              {label}
            </label>
          )}

          <input
            className={`nhsuk-input nhsuk-date-input__input nhsuk-input--width-2`}
            ref={ref}
            inputMode={'numeric'}
            id={id}
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...props}
          />
        </>
      );
    }

    return (
      <>
        {label && (
          <label className="nhsuk-label" htmlFor={id}>
            {label}
          </label>
        )}
        <div className="nhsuk-input__wrapper">
          {prefix && (
            <div className="nhsuk-input__prefix" aria-hidden="true">
              {prefix}
            </div>
          )}

          <input
            className={`nhsuk-input ${width !== undefined ? `nhsuk-input--width-${width}` : ''}`}
            ref={ref}
            id={id}
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...props}
          />
          {suffix && (
            <div className="nhsuk-input__suffix" aria-hidden="true">
              {suffix}
            </div>
          )}
        </div>
      </>
    );
  },
);
TextInput.displayName = 'TextInput';

export default TextInput;
