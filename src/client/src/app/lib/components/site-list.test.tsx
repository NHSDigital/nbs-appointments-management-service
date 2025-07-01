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
    expect(screen.queryByRole('cell', { name: 'Site Alpha' })).toBeVisible();
    expect(screen.queryByRole('cell', { name: 'Site Beta' })).toBeVisible();
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
    const dataRows = rows.slice(1);

    const firstCell = within(dataRows[0]).getAllByRole('cell')[0];
    expect(within(firstCell).getByText('Site Beta')).toBeInTheDocument();

    const secondCell = within(dataRows[1]).getAllByRole('cell')[0];
    expect(within(secondCell).getByText('Site Lima')).toBeInTheDocument();

    const thirdCell = within(dataRows[2]).getAllByRole('cell')[0];
    expect(within(thirdCell).getByText('Site November')).toBeInTheDocument();

    const fourthCell = within(dataRows[3]).getAllByRole('cell')[0];
    expect(within(fourthCell).getByText('Site Zulu')).toBeInTheDocument();
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
    expect(screen.getByRole('link', { name: 'View' })).toHaveAttribute(
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

    expect(screen.getAllByRole('row')).toHaveLength(5);
    expect(screen.getByRole('cell', { name: 'Site Zulu' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Lima' })).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: 'Site November' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Beta' })).toBeInTheDocument();

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Beta');

    await waitFor(() => {
      const rows = screen.getAllByRole('row');
      // Skip header row
      const dataRows = rows.slice(1);
      expect(dataRows).toHaveLength(1);

      const firstCell = within(dataRows[0]).getAllByRole('cell')[0];
      expect(within(firstCell).getByText('Site Beta')).toBeInTheDocument();
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

    expect(screen.getAllByRole('row')).toHaveLength(5);
    expect(screen.getByRole('cell', { name: 'Site Zulu' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Lima' })).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: 'Site November' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Beta' })).toBeInTheDocument();

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, 'Be');

    await waitFor(() => {
      const rows = screen.getAllByRole('row');
      // Skip header row
      const dataRows = rows.slice(1);
      expect(dataRows).toHaveLength(4);
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

    expect(screen.getAllByRole('row')).toHaveLength(5);
    expect(screen.getByRole('cell', { name: 'Site Zulu' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Lima' })).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: 'Site November' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Beta' })).toBeInTheDocument();

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1004');

    await waitFor(() => {
      const rows = screen.getAllByRole('row');
      // Skip header row
      const dataRows = rows.slice(1);
      expect(dataRows).toHaveLength(1);

      const firstCell = within(dataRows[0]).getAllByRole('cell')[0];
      expect(within(firstCell).getByText('Site Beta')).toBeInTheDocument();
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

    expect(screen.getAllByRole('row')).toHaveLength(5);
    expect(screen.getByRole('cell', { name: 'Site Zulu' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Lima' })).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: 'Site November' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Site Beta' })).toBeInTheDocument();

    const searchInput = screen.getByRole('textbox', {
      name: 'site-search',
    });
    await user.type(searchInput, '1005');

    await waitFor(() => {
      expect(screen.getAllByRole('row')).toHaveLength(1);
    });
  });
});
