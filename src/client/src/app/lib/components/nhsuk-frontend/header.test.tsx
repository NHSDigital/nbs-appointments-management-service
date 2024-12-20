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
      screen.getByRole('link', { name: 'Manage Your Appointments' }),
    ).toBeInTheDocument();
  });

  it('renders children', () => {
    render(
      <Header>
        <div>Some child content</div>
      </Header>,
    );

    expect(screen.getByText('Some child content')).toBeInTheDocument();
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

    expect(screen.getByRole('navigation')).toBeInTheDocument();
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
});
