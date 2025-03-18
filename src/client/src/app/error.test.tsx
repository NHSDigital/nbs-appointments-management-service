import { screen } from '@testing-library/react';
import render from '@testing/render';
import ErrorPage from './error';
import { MyaError, UnauthorizedError } from '@types';

describe('Error Page', () => {
  it('renders', () => {
    render(<ErrorPage error={new MyaError('Test error')} reset={() => {}} />);
  });

  it('shows the correct title', async () => {
    render(<ErrorPage error={new MyaError('Test error')} reset={() => {}} />);
    expect(
      screen.getByRole('heading', {
        name: 'Sorry, there is a problem with this service',
      }),
    ).toBeVisible();
  });

  it('shows a different title if the error was a 403', async () => {
    render(<ErrorPage error={new UnauthorizedError()} reset={() => {}} />);
    expect(
      screen.getByRole('heading', {
        name: 'You cannot access this page',
      }),
    ).toBeVisible();
  });

  it('shows the correct breadcrumbs including title', async () => {
    render(<ErrorPage error={new MyaError('Test error')} reset={() => {}} />);

    expect(screen.getByRole('link', { name: 'Home' })).toBeVisible();
    expect(screen.getByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      '/sites',
    );
  });
});
