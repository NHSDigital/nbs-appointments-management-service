import { render, screen } from '@testing-library/react';
import AssignRoles from './assign-roles';
import { Role, RoleAssignment, User } from '@types';
import { getMockUserAssignments, mockRoles } from '../../../../testing/data';
import { fetchRoles, fetchUsers } from '@services/appointmentsService';

jest.mock('./assign-roles-form', () => {
  const MockForm = ({
    site,
    user,
    roles,
    assignments,
  }: {
    site: string;
    user: string;
    roles: Role[];
    assignments: RoleAssignment[];
  }) => {
    return (
      <>
        <div>Assign Roles Form</div>
        {assignments.map(a => (
          <div key={a.role}>assignment={a.role}</div>
        ))}
        {roles.map(r => (
          <div key={r.id}>role={r.id}</div>
        ))}
        <div>site={site}</div>
        <div>user={user}</div>
      </>
    );
  };
  return MockForm;
});

jest.mock('../../../../lib/roles');
jest.mock('../../../../lib/users');

const fetchUsersMock = fetchUsers as jest.Mock<Promise<User[]>>;
const fetchRolesMock = fetchRoles as jest.Mock<Promise<Role[]>>;
const mockSiteId = 'TEST';

describe('AssignRoles', () => {
  beforeEach(() => {
    fetchUsersMock.mockResolvedValue(getMockUserAssignments(mockSiteId));
    fetchRolesMock.mockResolvedValue(mockRoles);
  });
  it('throws error when rendered without user', async () => {
    await expect(
      AssignRoles({
        params: { site: 'TEST' },
        searchParams: {},
      }),
    ).rejects.toThrow('You must specify a valid NHS email address');
  });

  it('throws error when rendered with non NHS email', async () => {
    await expect(
      AssignRoles({
        params: { site: 'TEST' },
        searchParams: { user: 'test@test.com' },
      }),
    ).rejects.toThrow('You must specify a valid NHS email address');
  });

  it('displays the email address of the user', async () => {
    const jsx = await AssignRoles({
      params: { site: 'TEST' },
      searchParams: { user: 'test@nhs.net' },
    });
    render(jsx);
    expect(screen.getByText('Email')).toBeVisible();
    expect(screen.getByText('test@nhs.net'));
  });
  it('calls fetch users with the correct site id', async () => {
    const jsx = await AssignRoles({
      params: { site: 'TEST' },
      searchParams: { user: 'test@nhs.net' },
    });
    render(jsx);
    expect(fetchUsersMock).toHaveBeenCalledWith('TEST');
  });
  it('passes the correct props', async () => {
    const jsx = await AssignRoles({
      params: { site: 'TEST' },
      searchParams: { user: 'test.one@nhs.net' },
    });
    render(jsx);
    expect(screen.getByText('assignment=role-1')).toBeVisible();
    expect(screen.getByText('assignment=role-2')).toBeVisible();
    expect(await screen.queryByText('assignment=role-3')).toBeNull();
    expect(screen.getByText('site=TEST')).toBeVisible();
    expect(screen.getByText('user=test.one@nhs.net')).toBeVisible();
    expect(screen.getByText('role=role-1')).toBeVisible();
    expect(screen.getByText('role=role-2')).toBeVisible();
    expect(screen.getByText('role=role-3')).toBeVisible();
  });
});
