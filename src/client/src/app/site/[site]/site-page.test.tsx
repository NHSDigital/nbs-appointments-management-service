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
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
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
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
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
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );
    verifySummaryListItem('Region', mockSite.region);
    verifySummaryListItem('ICB', mockSite.integratedCareBoard);

    expect(screen.queryByText('Region One')).not.toBeInTheDocument();
    expect(
      screen.queryByText('Integrated Care Board One'),
    ).not.toBeInTheDocument();
  });

  it('shows the user management page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Manage users' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Manage users' })).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/users`,
    );
  });

  it('does not show the user management page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Manage users' }),
    ).not.toBeInTheDocument();
  });

  it('shows the site management page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: 'Change site details and accessibility information',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Change site details and accessibility information',
      }),
    ).toHaveAttribute('href', `/site/${mockSite.id}/details`);
  });

  it('does not show the site management page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.queryByRole('link', {
        name: 'Change site details and accessibility information',
      }),
    ).not.toBeInTheDocument();
  });

  // TODO: Maybe parameterise these tests over permission/card pairs
  it('shows the create availability page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockAllPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Create availability' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create availability' }),
    ).toHaveAttribute('href', `/site/${mockSite.id}/create-availability`);
  });

  it('does not show the create availability page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Create Availability' }),
    ).not.toBeInTheDocument();
  });

  it('does not show any links when the user may not see any of them', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(
      screen.queryByRole('link', {
        name: 'View availability and manage appointments for your site',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Create Availability' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Change site details and accessibility information',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', {
        name: 'Manage users',
      }),
    ).not.toBeInTheDocument();
  });

  it('renders single value', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage
        site={mockSite}
        permissions={mockNonManagerPermissions}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
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
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    verifySummaryListItem('Phone Number', '0118 999 88199 9119 725 3');
    verifySummaryListItem('Address', ['Delta Street,', 'London']);
  });
});
