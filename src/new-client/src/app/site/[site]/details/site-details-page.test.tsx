import { render, screen } from '@testing-library/react';
import SiteDetailsPage from './site-details-page';
import { AttributeDefinition, AttributeValue } from '@types';
import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';
import {
  mockAttributeDefinitions,
  mockAttributeValues,
  mockSite,
} from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchAttributeDefinitionsMock = fetchAttributeDefinitions as jest.Mock<
  Promise<AttributeDefinition[]>
>;

jest.mock('@services/appointmentsService');
const fetchSiteAttributeValuesMock = fetchSiteAttributeValues as jest.Mock<
  Promise<AttributeValue[]>
>;

describe('Manage Attributes Page', () => {
  beforeEach(() => {
    fetchAttributeDefinitionsMock.mockResolvedValue(mockAttributeDefinitions);
    fetchSiteAttributeValuesMock.mockResolvedValue(mockAttributeValues);
  });

  it('renders', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:manage', 'site:view'],
    });
    render(jsx);

    expect(screen.getByRole('heading', { name: 'Access needs' })).toBeVisible();
  });

  it('displays the status of each accessibility attribute', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:manage', 'site:view'],
    });
    render(jsx);

    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 1' }),
    ).toBeVisible();
    expect(
      screen.getByRole('term', { name: 'Accessibility attribute 2' }),
    ).toBeVisible();

    expect(
      screen.getByRole('definition', { name: 'Status: Active' }),
    ).toBeVisible();
    expect(
      screen.getByRole('definition', { name: 'Status: Inactive' }),
    ).toBeVisible();
  });

  it('contains a manual go back button which returns to the site page', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:manage', 'site:view'],
    });
    render(jsx);

    expect(screen.getAllByRole('link', { name: 'Go back' })[0]).toBeVisible();

    expect(screen.getAllByRole('link', { name: 'Go back' })[0]).toHaveAttribute(
      'href',
      `/site/${mockSite.id}`,
    );
  });

  it('shows the manage site details button if the user has permission', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:manage', 'site:view'],
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Manage site details' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Manage site details' }),
    ).toHaveAttribute('href', `/site/${mockSite.id}/details/edit-attributes`);
  });

  it('hides the manage site details button if the user does not have permission', async () => {
    const jsx = await SiteDetailsPage({
      site: mockSite,
      permissions: ['site:view'],
    });
    render(jsx);

    expect(
      screen.queryByRole('link', { name: 'Manage site details' }),
    ).toBeNull();
  });
});
