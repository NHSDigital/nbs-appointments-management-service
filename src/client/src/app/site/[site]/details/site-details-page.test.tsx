import { render, screen } from '@testing-library/react';
import SiteDetailsPage from './site-details-page';
import {
  AccessibilityDefinition,
  FeatureFlag,
  ServerActionResult,
  Site,
} from '@types';
import {
  fetchAccessibilityDefinitions,
  fetchFeatureFlag,
  fetchSite,
} from '@services/appointmentsService';
import {
  mockAccessibilityDefinitions,
  mockSite,
  mockSites,
  mockWellKnownOdsCodeEntries,
} from '@testing/data';
import { verifyV10SummaryListItem } from '@components/nhsuk-frontend/summary-list.test';
import asServerActionResult from '@testing/asServerActionResult';

jest.mock('@services/appointmentsService');
const fetchAccessibilityDefinitionsMock =
  fetchAccessibilityDefinitions as jest.Mock<
    Promise<ServerActionResult<AccessibilityDefinition[]>>
  >;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<ServerActionResult<Site>>>;

jest.mock('@services/appointmentsService');
const fetchFeatureFlagMock = fetchFeatureFlag as jest.Mock<
  Promise<ServerActionResult<FeatureFlag>>
>;

describe('Site Details Page', () => {
  beforeEach(() => {
    fetchAccessibilityDefinitionsMock.mockResolvedValue(
      asServerActionResult(mockAccessibilityDefinitions),
    );
    fetchSiteMock.mockResolvedValue(asServerActionResult(mockSite));
    fetchFeatureFlagMock.mockResolvedValue(
      asServerActionResult({
        enabled: true,
      }),
    );
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

    verifyV10SummaryListItem('Name', mockSite.name);
    verifyV10SummaryListItem('Address', mockSite.address);
    verifyV10SummaryListItem(
      'Latitude',
      mockSite.location.coordinates[1].toString(),
    );
    verifyV10SummaryListItem(
      'Longitude',
      mockSite.location.coordinates[0].toString(),
    );
    verifyV10SummaryListItem('Phone Number', mockSite.phoneNumber);
  });

  it('shows the edit site details hyperlink if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Edit site details (Site details)' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Edit site details (Site details)' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/details/edit-details`,
    );
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

    verifyV10SummaryListItem('ODS code', mockSite.odsCode);
    verifyV10SummaryListItem('ICB', 'Integrated Care Board One');
    verifyV10SummaryListItem('Region', 'Region One');

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

    verifyV10SummaryListItem('ODS code', mockSite.odsCode);
    verifyV10SummaryListItem('ICB', mockSite.integratedCareBoard);
    verifyV10SummaryListItem('Region', mockSite.region);

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

    verifyV10SummaryListItem('Accessibility attribute 1', 'Yes');
    verifyV10SummaryListItem('Accessibility attribute 2', 'No');
  });

  it('shows the edit access needs hyperlink if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Edit access needs (Access needs)' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Edit access needs (Access needs)' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/details/edit-accessibilities`,
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
      screen.queryByRole('link', { name: 'Edit access needs (Access needs)' }),
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
      screen.getByRole('link', {
        name: 'Edit information for citizens (Information for citizens)',
      }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', {
        name: 'Edit information for citizens (Information for citizens)',
      }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/details/edit-information-for-citizens`,
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
      screen.queryByRole('link', {
        name: 'Edit information for citizens (Information for citizens)',
      }),
    ).toBeNull();
  });

  it('renders the site status when the feature flag is enabled', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:view', 'site:manage'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    verifyV10SummaryListItem('Status', 'Online');

    expect(
      screen.getByRole('link', { name: 'Change site status (Site details)' }),
    ).toBeInTheDocument();
  });

  it('hides the change site status link when feature toggle is disabled', async () => {
    fetchFeatureFlagMock.mockResolvedValue(
      asServerActionResult({
        enabled: false,
      }),
    );

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

  it('ensures access needs are correct when they have mixed casing', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSites[5].id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    verifyV10SummaryListItem('Accessibility attribute 1', 'Yes');
  });
});
