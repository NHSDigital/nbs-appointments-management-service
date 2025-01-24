import { render, screen } from '@testing-library/react';
import SiteDetailsPage from './site-details-page';
import { AttributeDefinition, SiteWithAttributes } from '@types';
import {
  fetchAttributeDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import {
  mockAttributeDefinitions,
  mockSite,
  mockSiteWithAttributes,
  mockWellKnownOdsCodeEntries,
} from '@testing/data';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

jest.mock('@services/appointmentsService');
const fetchAttributeDefinitionsMock = fetchAttributeDefinitions as jest.Mock<
  Promise<AttributeDefinition[]>
>;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<SiteWithAttributes>>;

describe('Site Details Page', () => {
  beforeEach(() => {
    fetchAttributeDefinitionsMock.mockResolvedValue(mockAttributeDefinitions);
    fetchSiteMock.mockResolvedValue(mockSiteWithAttributes);
  });

  it('renders', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

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
      screen.getByRole('heading', { level: 2, name: 'Site Details' }),
    ).toBeVisible();

    verifySummaryListItem('Address', mockSite.address);
    verifySummaryListItem(
      'Latitude',
      mockSite.location.coordinates[0].toString(),
    );
    verifySummaryListItem(
      'Longitude',
      mockSite.location.coordinates[1].toString(),
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

  it('displays the admin site details - ODS well known present', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { level: 2, name: 'Admin Details' }),
    ).toBeVisible();

    verifySummaryListItem('ODS code', mockSite.odsCode);
    verifySummaryListItem('ICB', 'Integrated Care Board One');
    verifySummaryListItem('Region', 'Region One');

    expect(
      screen.queryByText(mockSite.integratedCareBoard),
    ).not.toBeInTheDocument();
    expect(screen.queryByText(mockSite.region)).not.toBeInTheDocument();
  });

  it('displays the admin site details - ODS well known absent', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: [],
    });
    render(jsx);

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

    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 1-term' }),
    ).toBeVisible();

    const attr1desc = screen.getByRole('definition', {
      name: 'Accessibility attribute 1-description',
    });

    expect(attr1desc).toBeVisible();
    expect(attr1desc).toHaveTextContent('Yes');

    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 2-term' }),
    ).toBeVisible();

    const attr2desc = screen.getByRole('definition', {
      name: 'Accessibility attribute 2-description',
    });

    expect(attr2desc).toBeVisible();
    expect(attr2desc).toHaveTextContent('No');
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
    ).toHaveAttribute('href', `/site/${mockSite.id}/details/edit-attributes`);
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
});
