import { render, screen, waitFor } from '@testing-library/react';
import { UsersPage } from './page';
import { getMockUserAssignments, mockRoles } from '../../../testing/data';

const mockSiteId = 'TEST';

describe('Users Page', () => {
  beforeEach(() => {});

  it('renders', async () => {
    render(
      <UsersPage
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
      />,
    );

    expect(
      screen.getByRole('link', {
        name: `Assign staff roles`,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('table', {
        name: `Manage your current site's staff roles`,
      }),
    ).toBeInTheDocument();
  });

  it('displays each user in the table', async () => {
    render(
      <UsersPage
        users={getMockUserAssignments(mockSiteId)}
        roles={mockRoles}
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
});
