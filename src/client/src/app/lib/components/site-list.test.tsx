import { render, screen, within } from '@testing-library/react';
import SiteList from '@components/site-list';
import { Site } from '@types';

describe('<SiteList>', () => {
  it('renders a link for each available site', async () => {
    const testSites: Site[] = [
      {
        id: '1000',
        name: 'Site Alpha',
        address: 'Alpha Street',
        integratedCareBoard: 'ICB0',
        region: 'R0',
      },
      {
        id: '1001',
        name: 'Site Beta',
        address: 'Beta Street',
        integratedCareBoard: 'ICB1',
        region: 'R1',
      },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.queryByRole('link', { name: 'Site Alpha' })).toBeVisible();
    expect(screen.queryByRole('link', { name: 'Site Beta' })).toBeVisible();
  });

  it('renders sites in ascending alphabetical order', async () => {
    const testSites: Site[] = [
      {
        id: '1001',
        name: 'Site Zulu',
        address: 'Zulu Street',
        integratedCareBoard: 'ICB1',
        region: 'R1',
      },
      {
        id: '1002',
        name: 'Site Lima',
        address: 'Lima Street',
        integratedCareBoard: 'ICB2',
        region: 'R2',
      },
      {
        id: '1003',
        name: 'Site November',
        address: 'November Street',
        integratedCareBoard: 'ICB3',
        region: 'R3',
      },
      {
        id: '1004',
        name: 'Site Beta',
        address: 'Beta Street',
        integratedCareBoard: 'ICB4',
        region: 'R4',
      },
    ];
    render(<SiteList sites={testSites} />);
    const siteNames = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(siteNames).toHaveLength(4);
    expect(siteNames[0]?.textContent).toEqual('Site Beta');
    expect(siteNames[1]?.textContent).toEqual('Site Lima');
    expect(siteNames[2]?.textContent).toEqual('Site November');
    expect(siteNames[3]?.textContent).toEqual('Site Zulu');
  });

  it('links to the correct page', async () => {
    const testSites: Site[] = [
      {
        id: '1000',
        name: 'Site Alpha',
        address: 'Alpha Street',
        integratedCareBoard: 'ICB0',
        region: 'R0',
      },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      '/site/1000',
    );
  });
});
