import { Footer } from '@nhsuk-frontend-components';
import { screen } from '@testing-library/react';
import render from '@testing/render';

describe('Footer', () => {
  it('renders', () => {
    render(
      <Footer supportLinks={[{ text: 'Contact us', href: '/contact-us' }]} />,
    );

    expect(screen.getByRole('contentinfo')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Contact us' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Contact us' })).toHaveAttribute(
      'href',
      '/contact-us',
    );
  });
});
