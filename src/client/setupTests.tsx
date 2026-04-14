import React from 'react';
import '@testing-library/jest-dom';
import { loadEnvConfig } from '@next/env';

loadEnvConfig(process.cwd());

jest.setTimeout(10000);

// Global mock for NHS UK React Components
// This prevents "Experimental VM Modules" errors caused by
// dynamic imports inside the library's Button component.
/* eslint-disable @typescript-eslint/no-explicit-any, react/jsx-props-no-spreading */
jest.mock('nhsuk-react-components', () => {
  const actual = jest.requireActual('nhsuk-react-components');

  return {
    ...actual,
    Button: ({ children, warning, ...props }: any) => (
      <button {...props} data-warning={warning?.toString()}>
        {children}
      </button>
    ),
  };
});
/* eslint-enable @typescript-eslint/no-explicit-any, react/jsx-props-no-spreading */
