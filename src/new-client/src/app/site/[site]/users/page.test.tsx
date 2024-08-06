/* eslint-disable @typescript-eslint/no-explicit-any */
import { render, screen, waitFor } from '@testing-library/react';
import { Role, User } from '@types';
import UsersPage from './page';
import { fetchRoles } from '../../../lib/roles';
import { fetchUsers } from '../../../lib/users';

jest.mock('../../../lib/roles');
jest.mock('../../../lib/users');

const fetchUsersMock = fetchUsers as jest.Mock<any>;
const fetchRolesMock = fetchRoles as jest.Mock<any>;

describe('<UserManagement />', () => {
  it('renders', async () => {
    fetchUsersMock.mockResolvedValue(mockUserAssignments);
    fetchRolesMock.mockResolvedValue(mockRoles);

    const jsx = await UsersPage({ params: { site: mockSiteId } });
    await render(jsx);

    expect(
      screen.getByRole('table', {
        name: "Manage your current site's staff roles",
      }),
    ).toBeInTheDocument();
  });

  it('displays each user in the table', async () => {
    fetchUsersMock.mockResolvedValue(mockUserAssignments);
    fetchRolesMock.mockResolvedValue(mockRoles);

    const jsx = await UsersPage({ params: { site: mockSiteId } });
    await render(jsx);

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

const mockSiteId = '1000';
const mockUserAssignments: User[] = [
  {
    id: 'test.one@nhs.net',
    roleAssignments: [
      { role: 'role-1', scope: `site:${mockSiteId}` },
      {
        role: 'role-2',
        scope: `site:${mockSiteId}`,
      },
    ],
  },
  {
    id: 'test.two@nhs.net',
    roleAssignments: [{ role: 'role-3', scope: `site:${mockSiteId}` }],
  },
];

const mockRoles: Role[] = [
  {
    displayName: 'Role 1',
    id: 'role-1',
    description: 'This is a short description of role 1.',
  },
  {
    displayName: 'Role 2',
    id: 'role-2',
    description: 'This is a short description of role 2.',
  },
  {
    displayName: 'Role 3',
    id: 'role-3',
    description: 'This is a short description of role 3.',
  },
];
