/* eslint-disable react/jsx-props-no-spreading */
import { DetailedHTMLProps, forwardRef, InputHTMLAttributes } from 'react';

type Props = {
  label: string;
  hint?: string;
  error?: string;
} & DetailedHTMLProps<InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>;
type Ref = HTMLInputElement;

export const Datepicker = forwardRef<Ref, Props>(
  ({ id, label, hint, error, ...props }, ref) => {
    return (
      <div className="nhsuk-form-group">
        <label className="nhsuk-label" htmlFor={id}>
          {label}
        </label>
        <div id={`${id}-hint`} className="nhsuk-hint">
          {hint}
        </div>
        {error && (
          <span className="nhsuk-error-message">
            <span className="nhsuk-u-visually-hidden">Error: </span>
            {error}
          </span>
        )}

        <input
          className="nhsuk-input nhsuk-date-input__input nhsuk-input--width-8"
          type="date"
          inputMode="numeric"
          aria-describedby={`${id}-hint`}
          autoComplete="off"
          id={id}
          ref={ref}
          {...props}
        />
      </div>
    );
  },
);
Datepicker.displayName = 'TextInput';

export default Datepicker;
