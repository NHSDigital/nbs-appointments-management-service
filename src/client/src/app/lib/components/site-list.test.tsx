import { render, screen, within } from '@testing-library/react';
import SiteList from '@components/site-list';
import { Site } from '@types';

describe('<SiteList>', () => {
  it('renders a link for each available site', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Alpha',
        address: 'Alpha Street',
        odsCode: '1000',
        integratedCareBoard: 'ICB0',
        region: 'R0',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Beta',
        address: 'Beta Street',
        odsCode: '1002',
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
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Zulu',
        address: 'Zulu Street',
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        odsCode: '1004',
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
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Alpha',
        address: 'Alpha Street',
        odsCode: '1000',
        integratedCareBoard: 'ICB0',
        region: 'R0',
      },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      '/site/95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
    );
  });
});
