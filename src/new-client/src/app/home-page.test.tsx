import { HomePage } from './home-page';
import { render, screen } from '@testing-library/react';
import { mockSites } from './testing/data';

describe('Home Page', () => {
  it('should render the home page', () => {
    render(<HomePage sites={mockSites} />);

    expect(
      screen.getByRole('heading', { name: 'Choose a site' }),
    ).toBeInTheDocument();

    mockSites.forEach(site => {
      expect(screen.getByRole('link', { name: site.name })).toBeInTheDocument();
      expect(screen.getByRole('link', { name: site.name })).toHaveAttribute(
        'href',
        `site/${site.id}`,
      );
    });
  });
});
