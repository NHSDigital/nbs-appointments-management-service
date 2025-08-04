import { render, screen } from '@testing-library/react';
import { SitePage } from './site-page';
import {
  mockAllPermissions,
  mockNonManagerPermissions,
  mockSites,
  mockWellKnownOdsCodeEntries,
} from '@testing/data';
import { verifySummaryListItem } from '@components/nhsuk-frontend/summary-list.test';

describe('Site Page', () => {
  it('displays a summary of the site', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        permissionsAtAnySite={[]}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
        siteSummaryEnabled={true}
      />,
    );

    expect(screen.getByText(mockSite.address)).toBeInTheDocument();
  });

  it('displays full name for region and integrated care board when present in the ODS codes', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        permissionsAtAnySite={[]}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
        siteSummaryEnabled={true}
      />,
    );

    verifySummaryListItem('Region', 'Region One');
    verifySummaryListItem('ICB', 'Integrated Care Board One');

    expect(screen.queryByText(mockSite.region)).not.toBeInTheDocument();
    expect(
      screen.queryByText(mockSite.integratedCareBoard),
    ).not.toBeInTheDocument();
  });

  it('displays region and integrated care board ODS codes when a full name is not provided', () => {
    const mockSite = mockSites[1];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        permissionsAtAnySite={[]}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
        siteSummaryEnabled={true}
      />,
    );
    verifySummaryListItem('Region', mockSite.region);
    verifySummaryListItem('ICB', mockSite.integratedCareBoard);

    expect(screen.queryByText('Region One')).not.toBeInTheDocument();
    expect(
      screen.queryByText('Integrated Care Board One'),
    ).not.toBeInTheDocument();
  });

  it('renders the site address', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        permissionsAtAnySite={[]}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
        siteSummaryEnabled={true}
      />,
    );

    verifySummaryListItem('Address', 'Alpha Street');
  });

  it('renders address and phone number', () => {
    const mockSite = mockSites[3];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        permissionsAtAnySite={[]}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
        siteSummaryEnabled={true}
      />,
    );

    verifySummaryListItem('Phone Number', '0118 999 88199 9119 725 3');
    verifySummaryListItem('Address', ['Delta Street,', 'London']);
  });

  it.each([
    [
      'availability:query',
      'View availability and manage appointments for your site',
      'view-availability',
    ],
    ['availability:setup', 'Create availability', 'create-availability'],
    [
      'site:manage',
      'Change site details and accessibility information',
      'details',
    ],
    [
      'site:view',
      'Change site details and accessibility information',
      'details',
    ],
    ['users:view', 'Manage users', 'users'],
    ['reports:sitesummary', 'Download reports', 'reports'],
  ])(
    'displays the correct cards when permissions are present',
    (permission: string, cardTitle: string, path: string) => {
      const mockSite = mockSites[0];

      render(
        <SitePage
          site={mockSite}
          permissions={[permission]}
          permissionsAtAnySite={
            permission === 'reports:sitesummary' ? ['reports:sitesummary'] : []
          }
          wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
          siteSummaryEnabled={true}
        />,
      );

      expect(screen.getByRole('link', { name: cardTitle })).toBeInTheDocument();

      if (path === 'reports') {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/${path}`,
        );
      } else {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/site/${mockSite.id}/${path}`,
        );
      }
    },
  );

  it.each([
    [
      ['availability:query'],
      'View availability and manage appointments for your site',
    ],
    [['availability:setup'], 'Create availability'],
    [
      ['site:manage', 'site:view'],
      'Change site details and accessibility information',
    ],
    [['users:view'], 'Manage users'],
    [['reports:sitesummary'], 'Download reports'],
  ])(
    'hides the correct cards when permissions are lacking',
    (permissions: string[], cardTitle: string) => {
      const mockSite = mockSites[0];

      render(
        <SitePage
          site={mockSite}
          permissions={mockAllPermissions.filter(p => !permissions.includes(p))}
          permissionsAtAnySite={[]}
          wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
          siteSummaryEnabled={true}
        />,
      );

      expect(
        screen.queryByRole('link', { name: cardTitle }),
      ).not.toBeInTheDocument();
    },
  );
});
