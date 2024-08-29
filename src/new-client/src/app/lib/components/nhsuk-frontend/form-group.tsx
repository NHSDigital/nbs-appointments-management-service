import { ReactNode } from 'react';
import FieldSet from './fieldset';

type Props = {
  children: ReactNode;
  error?: string;
  legend?: string;
};

const FormGroup = ({ children, error, legend }: Props) => {
  return (
    <div
      className={`nhsuk-form-group ${error ? 'nhsuk-form-group--error' : ''}`}
    >
      {legend ? (
        <FieldSet legend={legend}>
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
