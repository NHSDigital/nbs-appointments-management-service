import React from 'react';
import { ReactNode, Children } from 'react';

type Props = {
  children: ReactNode;
  heading: string;
  hint: string;
  id: string;
};

const DateInput = ({ heading, hint, id, children }: Props) => {
  const childrenArray = Children.toArray(children);
  if (childrenArray.length !== 3) {
    throw new Error(
      'A DateInput component must have exactly 3 children. The first should be a day input, the second a month input, and the third a year input.',
    );
  }

  return (
    <fieldset
      className="nhsuk-fieldset"
      aria-describedby={`${id}-hint`}
      role="group"
    >
      <legend className="nhsuk-fieldset__legend nhsuk-label--l">
        <h1 className="nhsuk-fieldset__heading">{heading}</h1>
      </legend>
      <div className="nhsuk-hint" id={`${id}-hint`}>
        {hint}
      </div>

      <div className="nhsuk-date-input" id={id}>
        <div className="nhsuk-date-input__item">
          <div className="nhsuk-form-group">{childrenArray[0]}</div>
        </div>
        <div className="nhsuk-date-input__item">
          <div className="nhsuk-form-group">
            <div className="nhsuk-form-group">{childrenArray[1]}</div>
          </div>
        </div>
        <div className="nhsuk-date-input__item">
          <div className="nhsuk-form-group">
            <div className="nhsuk-form-group">{childrenArray[2]}</div>
          </div>
        </div>
      </div>
    </fieldset>
  );
};

export default DateInput;
