import React from 'react';
import '@testing-library/jest-dom';
import { loadEnvConfig } from '@next/env';

loadEnvConfig(process.cwd());

jest.setTimeout(10000);

// Global mock for NHS UK React Components
// This prevents "Experimental VM Modules" errors caused by
// dynamic imports inside the library's Button and Radios components.
/* eslint-disable @typescript-eslint/no-explicit-any, react/jsx-props-no-spreading */
jest.mock('nhsuk-react-components', () => {
  const actual = jest.requireActual('nhsuk-react-components');

  const MockRadios = ({ children, legend, error, ...props }: any) => (
    <div {...props}>
      {legend && <legend>{legend}</legend>}
      {error && <span className="nhsuk-error-message">{error}</span>}
      {children}
    </div>
  );

  const MockRadiosItem = React.forwardRef(
    ({ children, id, value, ...props }: any, ref: any) => (
      <div>
        <input type="radio" id={id} value={value} ref={ref} {...props} />
        <label htmlFor={id}>{children}</label>
      </div>
    ),
  );

  MockRadiosItem.displayName = 'MockRadiosItem';
  MockRadios.Item = MockRadiosItem;

  const MockCheckboxes = ({ children, ...props }: any) => (
    <div {...props}>{children}</div>
  );

  const MockCheckboxesItem = React.forwardRef(
    ({ children, id, value, ...props }: any, ref: any) => (
      <div>
        <input
          type="checkbox"
          id={id}
          value={value}
          ref={ref}
          label={typeof children === 'string' ? children : undefined}
          {...props}
        />
        <label htmlFor={id}>{children}</label>
      </div>
    ),
  );
  MockCheckboxesItem.displayName = 'MockCheckboxesItem';
  MockCheckboxes.Item = MockCheckboxesItem;

  return {
    ...actual,
    Button: ({ children, warning, secondary, ...props }: any) => (
      <button {...props} data-warning={warning?.toString()}>
        {children}
      </button>
    ),
    Radios: MockRadios,
    Checkboxes: MockCheckboxes,
  };
});
/* eslint-enable @typescript-eslint/no-explicit-any, react/jsx-props-no-spreading */
