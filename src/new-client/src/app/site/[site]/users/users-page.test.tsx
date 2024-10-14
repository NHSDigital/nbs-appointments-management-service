import { render, screen, waitFor } from '@testing-library/react';
import { UsersPage } from './users-page';
import {
  getMockUserAssignments,
  mockAllPermissions,
  mockAuditerPermissions,
  mockRoles,
  mockUserProfile,
} from '@testing/data';

const mockSiteId = 'TEST';

describe('Users Page', () => {
  beforeEach(() => {});

  it('renders', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: `Assign staff roles`,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText("Manage your current site's staff roles"),
    ).toBeVisible();
  });

  it('displays each user in the table', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
      />,
    );

    await waitFor(() => {
      expect(screen.getAllByRole('row').length).toBe(3);
    });

    expect(
      screen.getByRole('cell', { name: 'test.one@nhs.net' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('cell', {
        name: 'Role 1 | Role 2',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('cell', { name: 'test.two@nhs.net' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Role 3' })).toBeInTheDocument();
  });

  it('displays the edit and remove buttons for each user if they may see it', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
      />,
    );

    expect(screen.getByRole('columnheader', { name: 'Manage' })).toBeVisible();
    expect(screen.getAllByRole('link', { name: 'Edit' }).length).toBe(2);
    expect(
      screen.getAllByRole('link', { name: 'Remove from this site' }).length,
    ).toBe(2);

    expect(
      screen.getByRole('link', { name: 'Assign staff roles' }),
    ).toBeVisible();
  });

  it('omits the edit and remove buttons for each user if they may not see it', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAuditerPermissions}
      />,
    );

    expect(screen.queryByRole('columnheader', { name: 'Manage' })).toBeNull();
    expect(screen.queryByRole('link', { name: 'Edit' })).toBeNull();
    expect(
      screen.queryByRole('link', { name: 'Remove from this site' }),
    ).toBeNull();

    expect(
      screen.queryByRole('link', { name: 'Assign staff roles' }),
    ).toBeNull();
  });

  it('Does not display the remove button for the current user', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
      />,
    );

    expect(
      screen.queryAllByRole('link', { name: 'Remove from this site' }),
    ).toHaveLength(1);

    expect(
      screen.getByRole('link', { name: 'Remove from this site' }),
    ).toHaveAttribute('href', 'users/remove?user=test.two@nhs.net');

    // Guard against changes in mock data by providing the current user is included in the list of mock users
    expect(getMockUserAssignments(mockSiteId).map(_ => _.id)).toContain(
      mockUserProfile.emailAddress,
    );
  });
});
