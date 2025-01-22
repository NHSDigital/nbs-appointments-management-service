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

    expect(
      screen.getByRole('definition', { name: mockSite.address }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('definition', {
        name: mockSite.location.coordinates[0].toString(),
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('definition', {
        name: mockSite.location.coordinates[1].toString(),
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('definition', { name: mockSite.phoneNumber }),
    ).toBeInTheDocument();
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

    expect(
      screen.getByRole('definition', { name: mockSite.odsCode }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: 'Integrated Care Board One' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: mockSite.integratedCareBoard }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: 'Region One' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: mockSite.region }),
    ).not.toBeInTheDocument();
  });

  it('displays the admin site details - ODS well known absent', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: [],
    });
    render(jsx);

    expect(
      screen.getByRole('heading', { level: 2, name: 'Admin Details' }),
    ).toBeVisible();

    expect(
      screen.getByRole('definition', { name: mockSite.odsCode }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: mockSite.integratedCareBoard }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('definition', { name: mockSite.region }),
    ).toBeInTheDocument();
  });

  it('displays the status of each accessibility attribute', async () => {
    const jsx = await SiteDetailsPage({
      siteId: mockSite.id,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 1' }),
    ).toBeVisible();
    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 2' }),
    ).toBeVisible();

    expect(screen.getByRole('definition', { name: 'Yes' })).toBeVisible();
    expect(screen.getByRole('definition', { name: 'Yes' })).toBeVisible();
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
