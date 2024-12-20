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

describe('Manage Attributes Page', () => {
  beforeEach(() => {
    fetchAttributeDefinitionsMock.mockResolvedValue(mockAttributeDefinitions);
    fetchSiteMock.mockResolvedValue(mockSiteWithAttributes);
  });

  it('renders', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:manage', 'site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(screen.getByRole('heading', { name: 'Access needs' })).toBeVisible();
  });

  it('displays the status of each accessibility attribute', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
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
      site: mockSite,
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
      site: mockSite,
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
      site: mockSite,
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
      site: mockSite,
      permissions: ['site:view'],
      wellKnownOdsEntries: mockWellKnownOdsCodeEntries,
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Edit information for citizens' }),
    ).toBeNull();
  });
});
