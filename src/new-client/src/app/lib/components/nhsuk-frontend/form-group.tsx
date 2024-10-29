import { ReactNode } from 'react';
import FieldSet from './fieldset';

type Props = {
  children: ReactNode;
  error?: string;
  legend?: string;
  hint?: string;
};

const FormGroup = ({ children, error, legend, hint }: Props) => {
  return (
    <div
      className={`nhsuk-form-group ${error ? 'nhsuk-form-group--error' : ''}`}
    >
      {legend ? (
        <FieldSet legend={legend}>
          {hint ? <div className="nhsuk-hint">{hint}</div> : null}
          {error && (
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error: </span>
              {error}
            </span>
          )}
          {children}
        </FieldSet>
      ) : (
        <>
          {hint ? <div className="nhsuk-hint">{hint}</div> : null}
          {error && (
            <span className="nhsuk-error-message">
              <span className="nhsuk-u-visually-hidden">Error: </span>
              {error}
            </span>
          )}
          {children}
        </>
      )}
    </div>
  );
};

export default FormGroup;
