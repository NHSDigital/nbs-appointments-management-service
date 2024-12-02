import '@testing-library/jest-dom';
import { loadEnvConfig } from '@next/env';

loadEnvConfig(process.cwd());

jest.setTimeout(10000);
