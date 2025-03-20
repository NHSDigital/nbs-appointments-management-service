import { render, screen } from '@testing-library/react';
import AssignRoles from './assign-roles';
import { Role, RoleAssignment, User } from '@types';
import { getMockUserAssignments, mockRoles } from '@testing/data';
import { fetchRoles, fetchUsers } from '@services/appointmentsService';
import { notFound } from 'next/navigation';

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

jest.mock('@services/appointmentsService');
const fetchUsersMock = fetchUsers as jest.Mock<Promise<User[]>>;
const fetchRolesMock = fetchRoles as jest.Mock<Promise<Role[]>>;
const mockSiteId = 'TEST';

jest.mock('next/navigation');
const notFoundMock = notFound as jest.Mock<never>;

describe('AssignRoles', () => {
  beforeEach(() => {
    fetchUsersMock.mockResolvedValue(getMockUserAssignments(mockSiteId));
    fetchRolesMock.mockResolvedValue(mockRoles);
  });

  it('returns not found when no user is provided', async () => {
    const jsx = await AssignRoles({
      params: { site: 'TEST' },
      searchParams: { user: undefined },
    });
    render(jsx);

    expect(notFoundMock).toHaveBeenCalled();
  });

  it.each([['test@nhs.net'], ['test@gmail.com']])(
    'displays the email address of the user',
    async (email: string) => {
      const jsx = await AssignRoles({
        params: { site: 'TEST' },
        searchParams: { user: email },
      });
      render(jsx);
      expect(screen.getByText('Email')).toBeVisible();
      expect(screen.getByText(email));
    },
  );

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
