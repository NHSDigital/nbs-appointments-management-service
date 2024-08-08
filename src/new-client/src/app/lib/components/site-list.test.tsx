import { render, screen } from '@testing-library/react';
import SiteList from '@components/site-list';
import { Site } from '@types';

describe('<SiteList>', () => {
  it('renders a link for each available site', async () => {
    const testSites: Site[] = [
      { id: '1000', name: 'Site Alpha', address: 'Alpha Street' },
      { id: '1001', name: 'Site Beta', address: 'Beta Street' },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.queryByRole('link', { name: 'Site Alpha' })).toBeVisible();
    expect(screen.queryByRole('link', { name: 'Site Beta' })).toBeVisible();
  });

  it('links to the correct page', async () => {
    const testSites: Site[] = [
      { id: '1000', name: 'Site Alpha', address: 'Alpha Street' },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      'site/1000',
    );
  });
});
