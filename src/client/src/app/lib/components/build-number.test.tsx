import render from '@testing/render';
import { screen } from '@testing-library/react';
import BuildNumber from './build-number';

describe('Build Number', () => {
  const OLD_ENV = process.env;

  beforeEach(() => {
    jest.resetModules();
    process.env = { ...OLD_ENV };
  });

  afterAll(() => {
    process.env = OLD_ENV;
  });

  it('Renders the build number', () => {
    process.env.BUILD_NUMBER = 'test-build-number';

    render(<BuildNumber />);

    expect(
      screen.getByText('Build number: test-build-number'),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Build number: test-build-number'),
    ).not.toBeVisible();
  });
});
