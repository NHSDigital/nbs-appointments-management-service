import { screen, waitFor, within } from '@testing-library/react';
import render from '@testing/render';
import SiteList from '@components/site-list';
import { Site } from '@types';

describe('<SiteList>', () => {
  it('renders a link for each available site', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Alpha',
        address: 'Alpha Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1000',
        integratedCareBoard: 'ICB0',
        region: 'R0',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
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
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1004',
        integratedCareBoard: 'ICB4',
        region: 'R4',
        accessibilities: [],
        informationForCitizens: '',
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
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        address: 'Alpha Street',
        odsCode: '1000',
        integratedCareBoard: 'ICB0',
        region: 'R0',
        accessibilities: [],
        informationForCitizens: '',
      },
    ];
    render(<SiteList sites={testSites} />);
    expect(screen.getByRole('link', { name: 'Site Alpha' })).toHaveAttribute(
      'href',
      '/site/95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
    );
  });

  it('filters sites on search input', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Zulu',
        address: 'Zulu Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1004',
        integratedCareBoard: 'ICB4',
        region: 'R4',
        accessibilities: [],
        informationForCitizens: '',
      },
    ];
    const { user } = render(<SiteList sites={testSites} />);

    const siteNames = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(siteNames).toHaveLength(4);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Beta');

    await waitFor(() => {
      const filteredSites = within(screen.getByRole('list')).getAllByRole(
        'listitem',
      );
      expect(filteredSites).toHaveLength(1);
      expect(filteredSites[0]?.textContent).toBe('Site Beta');
    });
  });

  it('does not filter results when search input is less than 3 characters', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Zulu',
        address: 'Zulu Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1004',
        integratedCareBoard: 'ICB4',
        region: 'R4',
        accessibilities: [],
        informationForCitizens: '',
      },
    ];
    const { user } = render(<SiteList sites={testSites} />);

    const siteNames = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(siteNames).toHaveLength(4);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Be');

    await waitFor(() => {
      const filteredSites = within(screen.getByRole('list')).getAllByRole(
        'listitem',
      );
      expect(filteredSites).toHaveLength(4);
    });
  });

  it('filters sites on exact match of ODS code', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Zulu',
        address: 'Zulu Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1004',
        integratedCareBoard: 'ICB4',
        region: 'R4',
        accessibilities: [],
        informationForCitizens: '',
      },
    ];
    const { user } = render(<SiteList sites={testSites} />);

    const siteNames = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(siteNames).toHaveLength(4);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1004');

    await waitFor(() => {
      const filteredSites = within(screen.getByRole('list')).getAllByRole(
        'listitem',
      );
      expect(filteredSites).toHaveLength(1);
      expect(filteredSites[0]?.textContent).toBe('Site Beta');
    });
  });

  it('cannot find site on partial ODS code search', async () => {
    const testSites: Site[] = [
      {
        id: '34e990af-5dc9-43a6-8895-b9123216d699',
        name: 'Site Zulu',
        address: 'Zulu Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1001',
        integratedCareBoard: 'ICB1',
        region: 'R1',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '95e4ca69-da15-45f5-9ec7-6b2ea50f07c8',
        name: 'Site Lima',
        address: 'Lima Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1002',
        integratedCareBoard: 'ICB2',
        region: 'R2',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: 'd79bec60-8968-4101-b553-67dec04e1019',
        name: 'Site November',
        address: 'November Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1003',
        integratedCareBoard: 'ICB3',
        region: 'R3',
        accessibilities: [],
        informationForCitizens: '',
      },
      {
        id: '90a9c1f2-83d0-4c40-9c7c-080d91c56e79',
        name: 'Site Beta',
        address: 'Beta Street',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
        odsCode: '1004',
        integratedCareBoard: 'ICB4',
        region: 'R4',
        accessibilities: [],
        informationForCitizens: '',
      },
    ];
    const { user } = render(<SiteList sites={testSites} />);

    const siteNames = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(siteNames).toHaveLength(4);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1005');

    await waitFor(() => {
      const filteredSites = within(screen.getByRole('list')).queryAllByRole(
        'listitem',
      );
      expect(filteredSites).toHaveLength(0);
    });
  });
});
