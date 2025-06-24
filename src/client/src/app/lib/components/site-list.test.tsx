import { screen, waitFor } from '@testing-library/react';
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
    expect(screen.getByRole('link', { name: /site alpha/i })).toBeVisible();
    expect(screen.getByRole('link', { name: /site beta/i })).toBeVisible();
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
    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(5); // 1 header + 4 sites
    expect(rows[1]?.textContent).toContain('Site Beta');
    expect(rows[2]?.textContent).toContain('Site Lima');
    expect(rows[3]?.textContent).toContain('Site November');
    expect(rows[4]?.textContent).toContain('Site Zulu');
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
    const link = screen.getByRole('link', { name: /site alpha/i });
    expect(link).toHaveAttribute(
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

    const initialRows = screen.getAllByRole('row');
    expect(initialRows.length).toBe(5); // 4 sites + header

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Beta');

    const searchButton = screen.getByRole('button', { name: /search/i });
    await user.click(searchButton);

    await waitFor(() => {
      const rowsAfterSearch = screen.getAllByRole('row');
      expect(rowsAfterSearch.length).toBe(2);
      expect(screen.getByText('Site Beta')).toBeInTheDocument();
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

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(5); // 4 sites + 1 header

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Be');

    await waitFor(() => {
      const filteredRows = screen.getAllByRole('row');
      expect(filteredRows).toHaveLength(5); // 4 sites + header
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

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(5);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1004');

    const searchButton = screen.getByRole('button', { name: /search/i });
    await user.click(searchButton);

    await waitFor(() => {
      const filteredRows = screen.getAllByRole('row');
      expect(filteredRows).toHaveLength(2); // header + 1 match
      expect(screen.getByText('Site Beta')).toBeInTheDocument();
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

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(5);

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1005');

    const searchButton = screen.getByRole('button', { name: /search/i });
    await user.click(searchButton);

    await waitFor(() => {
      const filteredRows = screen.queryAllByRole('row');
      expect(filteredRows).toHaveLength(1); // only header remains
    });
  });
});
