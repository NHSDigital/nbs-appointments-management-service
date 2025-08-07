import { render, screen } from '@testing-library/react';
import SiteDetailsPage from './site-details-page';
import { AccessibilityDefinition, FeatureFlag, Site } from '@types';
import {
  fetchAccessibilityDefinitions,
  fetchFeatureFlag,
  fetchSite,
} from '@services/appointmentsService';
import {
  mockAccessibilityDefinitions,
  mockSite,
  mockWellKnownOdsCodeEntries,
} from '@testing/data';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

jest.mock('@services/appointmentsService');
const fetchAccessibilityDefinitionsMock =
  fetchAccessibilityDefinitions as jest.Mock<
    Promise<AccessibilityDefinition[]>
  >;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<Site>>;

jest.mock('@services/appointmentsService');
const fetchFeatureFlagMock = fetchFeatureFlag as jest.Mock<
  Promise<FeatureFlag>
>;

describe('Site Details Page', () => {
  beforeEach(() => {
    fetchAccessibilityDefinitionsMock.mockResolvedValue(
      mockAccessibilityDefinitions,
    );
    fetchSiteMock.mockResolvedValue(mockSite);
    fetchFeatureFlagMock.mockResolvedValue({
      enabled: true,
    });
  });

  it('renders', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(screen.getByRole('heading', { name: 'Site details' })).toBeVisible();
    expect(screen.getByRole('heading', { name: 'Access needs' })).toBeVisible();
  });

  it('displays the core site details', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { level: 2, name: 'Site details' }),
    ).toBeVisible();

    verifySummaryListItem('Name', mockSite.name);
    verifySummaryListItem('Address', mockSite.address);
    verifySummaryListItem(
      'Latitude',
      mockSite.location.coordinates[1].toString(),
    );
    verifySummaryListItem(
      'Longitude',
      mockSite.location.coordinates[0].toString(),
    );
    verifySummaryListItem('Phone Number', mockSite.phoneNumber);
  });

  it('shows the edit site details hyperlink if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Edit site details' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Edit site details' }),
    ).toHaveAttribute('href', `/site/${mockSite.id}/details/edit-details`);
  });

  it('hides the edit site details hyperlink if the user does not have permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: [],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Edit site details' }),
    ).not.toBeInTheDocument();
  });

  it('displays the site reference details - ODS well known present', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { level: 2, name: 'Site reference details' }),
    ).toBeVisible();

    verifySummaryListItem('ODS code', mockSite.odsCode);
    verifySummaryListItem('ICB', 'Integrated Care Board One');
    verifySummaryListItem('Region', 'Region One');

    expect(
      screen.queryByText(mockSite.integratedCareBoard),
    ).not.toBeInTheDocument();
    expect(screen.queryByText(mockSite.region)).not.toBeInTheDocument();
  });

  it('displays the site reference details - ODS well known absent', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: [],
    });
    render(jsx);

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Site reference details',
      }),
    ).toBeVisible();

    verifySummaryListItem('ODS code', mockSite.odsCode);
    verifySummaryListItem('ICB', mockSite.integratedCareBoard);
    verifySummaryListItem('Region', mockSite.region);

    expect(
      screen.queryByText('Integrated Care Board One'),
    ).not.toBeInTheDocument();
    expect(screen.queryByText('Region One')).not.toBeInTheDocument();
  });

  it('displays the status of each accessibility attribute', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    verifySummaryListItem('Accessibility attribute 1', 'Yes');
    verifySummaryListItem('Accessibility attribute 2', 'No');
  });

  it('shows the edit access needs hyperlink if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Edit access needs' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Edit access needs' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/details/edit-accessibilities`,
    );
  });

  it('hides the edit access needs hyperlink if the user does not have permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Edit access needs' }),
    ).toBeNull();
  });

  it('shows the edit information for citizens hyperlink if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Edit information for citizens' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Edit information for citizens' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/details/edit-information-for-citizens`,
    );
  });

  it('hides the edit information for citizens hyperlink if the user does not have permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Edit information for citizens' }),
    ).toBeNull();
  });

  it('renders the site status when the feature flag is enabled', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:view', 'site:manage'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    verifySummaryListItem('Status', 'Online');

    expect(
      screen.getByRole('link', { name: 'Change site status' }),
    ).toBeInTheDocument();
  });

  it('hides the change site status link when feature toggle is disabled', async () => {
    fetchFeatureFlagMock.mockResolvedValue({
      enabled: false,
    });

    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:view', 'site:manage'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Change site status' }),
    ).not.toBeInTheDocument();
  });
});
