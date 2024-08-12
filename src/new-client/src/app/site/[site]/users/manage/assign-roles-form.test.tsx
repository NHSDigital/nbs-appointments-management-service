/* eslint-disable import/no-extraneous-dependencies */
import { render, screen } from '@testing-library/react';
import AssignRolesForm from './assign-roles-form';
import { Role, RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import userEvent from '@testing-library/user-event';
import { saveUserRoleAssignments } from '../../../../lib/users';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock<any>;
const mockReplace = jest.fn();

jest.mock('../../../../lib/users');
const mockSaveUserRoleAssignments = saveUserRoleAssignments as jest.Mock<
  Promise<void>
>;

describe('Assign Roles Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });
  it('displays a check box for each available role', () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );

    expect(screen.getByRole('checkbox', { name: 'Role 1' })).toBeVisible();
    expect(screen.getByRole('checkbox', { name: 'Role 2' })).toBeVisible();
    expect(screen.getByRole('checkbox', { name: 'Role 3' })).toBeVisible();
  });
  it('checks the correct options', () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={mockAssignments}
        roles={mockRoles}
      />,
    );

    expect(screen.getByRole('checkbox', { name: 'Role 1' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Role 2' })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Role 3' })).toBeChecked();
  });
  it('display a validation error when attempting to submit the form with no roles selected', async () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const submitButton = screen.getByRole('button', { name: 'save user' });
    await userEvent.click(submitButton);

    expect(
      screen.getByText('You have not selected any roles for this user'),
    ).toBeVisible();
  });
  it('returns the user to the users list when they cancel', async () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const cancelButton = screen.getByRole('button', { name: 'cancel' });
    await userEvent.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });
  it('calls the save function when saved', async () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const checkBox = screen.getByRole('checkbox', { name: 'Role 1' });
    await userEvent.click(checkBox);
    const saveButton = screen.getByRole('button', { name: 'save user' });
    await userEvent.click(saveButton);

    expect(mockSaveUserRoleAssignments).toHaveBeenCalledWith(
      'TEST',
      'test@nhs.net',
      ['role-1'],
    );
  });
});

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

const mockAssignments = [
  { role: 'role-1', scope: 'site:TEST' },
  {
    role: 'role-3',
    scope: 'site:TEST',
  },
];
