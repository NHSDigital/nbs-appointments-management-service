import { SitesPage } from './sites-page';
import { render, screen } from '@testing-library/react';
import { mockSites } from '@testing/data';

describe('Sites Page', () => {
  it('should render the home page', () => {
    render(<SitesPage sites={mockSites} />);

    expect(
      screen.getByRole('heading', { name: 'Choose a site' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Site Alpha' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      `/site/34e990af-5dc9-43a6-8895-b9123216d699`,
    );

    expect(screen.getByRole('link', { name: 'Site Beta' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Beta' })).toHaveAttribute(
      'href',
      `/site/95e4ca69-da15-45f5-9ec7-6b2ea50f07c8`,
    );

    expect(
      screen.getByRole('link', { name: 'Site Gamma' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Site Gamma' })).toHaveAttribute(
      'href',
      `/site/d79bec60-8968-4101-b553-67dec04e1019`,
    );
  });
});
