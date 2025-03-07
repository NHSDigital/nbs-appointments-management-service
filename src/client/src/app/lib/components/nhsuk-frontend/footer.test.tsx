import { Footer } from '@nhsuk-frontend-components';
import { screen } from '@testing-library/react';
import render from '@testing/render';

describe('Footer', () => {
  it('renders', () => {
    render(
      <Footer supportLinks={[{ text: 'Contact us', href: '/contact-us' }]} />,
    );

    expect(screen.getByRole('contentinfo')).toBeInTheDocument();

    const linkElement = screen.getByRole('link', { name: 'Contact us' });

    expect(linkElement).toBeInTheDocument();
    expect(linkElement).toHaveAttribute('href', '/contact-us');
    expect(linkElement).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('renders children', () => {
    render(<Footer>Hello World</Footer>);

    expect(screen.getByText('Hello World')).toBeInTheDocument();
  });
});
