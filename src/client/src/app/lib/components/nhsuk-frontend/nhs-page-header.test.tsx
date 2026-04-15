import { render, screen } from '@testing-library/react';
import { Header } from '@nhsuk-frontend-components';

describe('Header', () => {
  it('renders', () => {
    render(<Header />);

    expect(screen.getByRole('banner')).toBeInTheDocument();
  });

  it('contains a link back to the home page', () => {
    render(<Header />);

    expect(
      screen.getByRole('link', {
        name: 'NHS Manage Your Appointments homepage',
      }),
    ).toBeInTheDocument();
  });

  it('renders navigation links', () => {
    render(
      <Header
        navigationLinks={[
          { href: '/link1', label: 'Link 1' },
          { href: '/link2', label: 'Link 2' },
        ]}
      />,
    );

    expect(screen.getByRole('link', { name: 'Link 1' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 1' })).toHaveAttribute(
      'href',
      '/link1',
    );

    expect(screen.getByRole('link', { name: 'Link 2' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link 2' })).toHaveAttribute(
      'href',
      '/link2',
    );
  });

  it('Displays a link with the site name if a site is provided', async () => {
    render(
      <Header
        showChangeSiteButton
        siteName="Test site"
        userEmail="test-user@test.com"
      />,
    );

    expect(screen.getByRole('link', { name: 'Test site' })).toBeInTheDocument();
  });

  it('Does not display site name link if no site is provided', async () => {
    render(<Header showChangeSiteButton userEmail="test.user@test.com" />);

    expect(screen.queryByRole('link', { name: 'Test site' })).toBeNull();
  });
});
