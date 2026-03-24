import { mockSecondaryLinks } from '@testing/data';
import { SecondaryNavigation } from './seconday-navigation';
import { render, screen } from '@testing-library/react';

describe('SecondaryNavigation tests', () => {
  it('renders links with correct details', () => {
    render(<SecondaryNavigation links={mockSecondaryLinks} />);

    expect(screen.getByRole('link', { name: 'Link One' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link One' })).toHaveAttribute(
      'href',
      '/link/one/url',
    );
    expect(screen.getByRole('link', { name: 'Link Two' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link Two' })).toHaveAttribute(
      'href',
      '/link/two/url',
    );
    expect(screen.getByRole('link', { name: 'Link Two' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(
      screen.getByRole('link', { name: 'Link Three' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link Three' })).toHaveAttribute(
      'href',
      '/link/three/url',
    );
  });
});
