import { render, screen, waitFor } from '@testing-library/react';
import { Role, User, UserProfile } from '@types';
import UsersPage from './page';
import { getMockUserAssignments, mockRoles } from '../../../testing/data';
import {
  fetchRoles,
  fetchUsers,
  fetchUserProfile,
} from '@services/appointmentsService';

jest.mock('@services/appointmentsService');

const fetchUsersMock = fetchUsers as jest.Mock<Promise<User[]>>;
const fetchRolesMock = fetchRoles as jest.Mock<Promise<Role[]>>;
const fetchUserProfileMock = fetchUserProfile as jest.Mock<
  Promise<UserProfile>
>;
const mockSiteId = 'TEST';

describe('UserManagement', () => {
  beforeEach(() => {
    fetchUsersMock.mockResolvedValue(getMockUserAssignments(mockSiteId));
    fetchRolesMock.mockResolvedValue(mockRoles);
    fetchUserProfileMock.mockResolvedValue({
      emailAddress: 'test@test.com',
      availableSites: [
        { id: '1000', name: 'Site Alpha', address: 'Alpha Street' },
        { id: '1001', name: 'Site Beta', address: 'Beta Street' },
        { id: 'TEST', name: 'Test site', address: 'Test street' },
      ],
    });
  });

  it('renders', async () => {
    const jsx = await UsersPage({ params: { site: mockSiteId } });
    render(jsx);

    expect(screen.getByText('Manage Staff Roles')).toBeVisible();

    expect(
      screen.getByText("Manage your current site's staff roles"),
    ).toBeVisible();
  });

  it('displays each user in the table', async () => {
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
