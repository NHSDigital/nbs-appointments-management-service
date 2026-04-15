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

  return {
    ...actual,
    Button: ({ children, warning, ...props }: any) => (
      <button {...props} data-warning={warning?.toString()}>
        {children}
      </button>
    ),
    Radios: MockRadios,
  };
});
/* eslint-enable @typescript-eslint/no-explicit-any, react/jsx-props-no-spreading */
