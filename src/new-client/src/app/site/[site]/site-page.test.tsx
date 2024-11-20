import { render, screen } from '@testing-library/react';
import { SitePage } from './site-page';
import {
  mockAllPermissions,
  mockNonManagerPermissions,
  mockSites,
} from '@testing/data';

describe('Site Page', () => {
  it('renders', () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} permissions={mockAllPermissions} />);

    expect(
      screen.getByRole('heading', { name: mockSite.name }),
    ).toBeInTheDocument();
  });

  it('displays a summary of the site', () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} permissions={mockAllPermissions} />);

    expect(screen.getByText(mockSite.address)).toBeInTheDocument();
  });

  it('shows the user management page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} permissions={mockAllPermissions} />);

    expect(
      screen.getByRole('link', { name: 'User management' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'User management' }),
    ).toHaveAttribute('href', `${mockSite.id}/users`);
  });

  it('does not show the user management page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage site={mockSite} permissions={mockNonManagerPermissions} />,
    );

    expect(
      screen.queryByRole('link', { name: 'User management' }),
    ).not.toBeInTheDocument();
  });

  it('shows the site management page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} permissions={mockAllPermissions} />);

    expect(
      screen.getByRole('link', { name: 'Site management' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Site management' }),
    ).toHaveAttribute('href', `${mockSite.id}/details`);
  });

  it('does not show the site management page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage site={mockSite} permissions={mockNonManagerPermissions} />,
    );

    expect(
      screen.queryByRole('link', { name: 'Site Management' }),
    ).not.toBeInTheDocument();
  });

  it('shows the create availability page if the user may see it', () => {
    const mockSite = mockSites[0];

    render(<SitePage site={mockSite} permissions={mockAllPermissions} />);

    expect(
      screen.getByRole('link', { name: 'Create availability' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create availability' }),
    ).toHaveAttribute('href', `${mockSite.id}/create-availability`);
  });

  it('does not show the create availability page if the user may not see it', () => {
    const mockSite = mockSites[0];

    render(
      <SitePage site={mockSite} permissions={mockNonManagerPermissions} />,
    );

    expect(
      screen.queryByRole('link', { name: 'Create Availability' }),
    ).not.toBeInTheDocument();
  });
});
