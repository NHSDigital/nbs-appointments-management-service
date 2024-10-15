import render from '@testing/render';
import { screen } from '@testing-library/react';
import { BackLink } from '@nhsuk-frontend-components';

describe('Back Link', () => {
  it('renders', () => {
    render(<BackLink href="/mock-route" />);

    expect(screen.getByText('Go back')).toBeInTheDocument();
  });

  it('renders with a link', () => {
    render(<BackLink href="/mock-route" />);

    expect(screen.getByRole('link', { name: 'Go back' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Go back' })).toHaveAttribute(
      'href',
      '/mock-route',
    );
  });
});
