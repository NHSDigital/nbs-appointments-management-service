const nextJest = require('next/jest.js');

const createJestConfig = nextJest({
  dir: './',
});

/** @type {import('jest').Config} */
const config = {
  collectCoverageFrom: ['src/**/*.{ts,tsx}', '!src/**/*.d.ts'],
  setupFilesAfterEnv: ['<rootDir>/setupTests.ts'],
  testMatch: [
    '<rootDir>/src/**/*.(spec|test).{js,jsx,ts,tsx}',
    '<rootDir>/test/**/*.(spec|test).{js,jsx,ts,tsx}',
  ],
  testEnvironment: 'jest-fixed-jsdom',
  testEnvironmentOptions: {
    url: 'http://localhost/',
  },
  moduleNameMapper: {
    '^.+\\.module\\.(css|sass|scss)$': 'identity-obj-proxy',
    '^axios$': '<rootDir>/node_modules/axios/dist/axios.js',
    '@types': '<rootDir>/src/app/lib/types/index',
    '@components/(.*)$': '<rootDir>/src/app/lib/components/$1',
    '@services/(.*)$': '<rootDir>/src/app/lib/services/$1',
    '@constants': '<rootDir>/src/constants',
  },
  modulePathIgnorePatterns: ['.next'],
  moduleFileExtensions: [
    'web.js',
    'js',
    'web.ts',
    'ts',
    'web.tsx',
    'tsx',
    'json',
    'web.jsx',
    'jsx',
    'node',
  ],
  resetMocks: true,
};

module.exports = createJestConfig(config);
