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

  it('renders sites in ascending alphabetical order', async () => {
    const testSites: Site[] = [
      { id: '1001', name: 'Site Zulu', address: 'Alpha Street' },
      { id: '1002', name: 'Site Lima', address: 'Beta Street' },
      { id: '1003', name: 'Site November', address: 'Beta Street' },
      { id: '1004', name: 'Site Beta', address: 'Beta Street' },
    ];
    render(<SiteList sites={testSites} />);
    const list = screen.getByRole('list');
    const siteNames = list.children;
    expect(siteNames.length).toBe(4);
    expect(siteNames.item(0)?.textContent).toEqual('Site Beta');
    expect(siteNames.item(1)?.textContent).toEqual('Site Lima');
    expect(siteNames.item(2)?.textContent).toEqual('Site November');
    expect(siteNames.item(3)?.textContent).toEqual('Site Zulu');
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
