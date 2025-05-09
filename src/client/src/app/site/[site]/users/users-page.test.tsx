import { render, screen, waitFor } from '@testing-library/react';
import { UsersPage } from './users-page';
import {
  getMockOktaUserAssignments,
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
        oktaEnabled={true}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: `Add user`,
      }),
    ).toBeInTheDocument();
  });

  it('displays each user in the table', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
        oktaEnabled={true}
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
        name: 'Beta Role | Charlie Role',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('cell', { name: 'test.two@nhs.net' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: 'Alpha Role' }),
    ).toBeInTheDocument();
  });

  it('displays the edit and remove buttons for each user if they may see it', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
        oktaEnabled={true}
      />,
    );

    expect(screen.getByRole('columnheader', { name: 'Manage' })).toBeVisible();
    expect(screen.getAllByRole('link', { name: 'Edit' }).length).toBe(1);
    expect(
      screen.getAllByRole('link', { name: 'Remove from this site' }).length,
    ).toBe(1);

    expect(screen.getByRole('button', { name: 'Add user' })).toBeVisible();
  });

  it('omits the edit and remove buttons for each user if they may not see it', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAuditerPermissions}
        oktaEnabled={true}
      />,
    );

    expect(screen.queryByRole('columnheader', { name: 'Manage' })).toBeNull();
    expect(screen.queryByRole('link', { name: 'Edit' })).toBeNull();
    expect(
      screen.queryByRole('link', { name: 'Remove from this site' }),
    ).toBeNull();

    expect(screen.queryByRole('button', { name: 'Add user' })).toBeNull();
  });

  it('Does not display the edit or remove buttons for the current user', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
        oktaEnabled={true}
      />,
    );

    expect(
      screen.queryAllByRole('link', { name: 'Remove from this site' }),
    ).toHaveLength(1);
    expect(screen.getAllByRole('link', { name: 'Edit' }).length).toBe(1);

    expect(
      screen.getByRole('link', { name: 'Remove from this site' }),
    ).toHaveAttribute('href', 'users/remove?user=test.two@nhs.net');

    // Guard against changes in mock data by providing the current user is included in the list of mock users
    expect(getMockUserAssignments(mockSiteId).map(_ => _.id)).toContain(
      mockUserProfile.emailAddress,
    );
  });

  it('Does not display the edit or remove buttons for okta users if okta is disabled - but does display them for nhs users', async () => {
    render(
      <UsersPage
        userProfile={mockUserProfile}
        users={getMockOktaUserAssignments(mockSiteId)}
        roles={mockRoles}
        permissions={mockAllPermissions}
        oktaEnabled={false}
      />,
    );

    expect(
      screen.queryAllByRole('link', { name: 'Remove from this site' }),
    ).toHaveLength(1);
    expect(screen.queryAllByRole('link', { name: 'Edit' }).length).toBe(1);

    expect(getMockUserAssignments(mockSiteId).map(_ => _.id)).toContain(
      mockUserProfile.emailAddress,
    );
  });
});
