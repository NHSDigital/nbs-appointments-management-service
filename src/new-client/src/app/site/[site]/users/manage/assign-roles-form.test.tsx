import { screen } from '@testing-library/react';
import AssignRolesForm from './assign-roles-form';
import { RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import { mockRoles } from '@testing/data';
import render from '@testing/render';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockSaveUserRoleAssignments = jest.spyOn(
  appointmentsService,
  'saveUserRoleAssignments',
);

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
    const { user } = render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const submitButton = screen.getByRole('button', {
      name: 'Confirm and save',
    });
    await user.click(submitButton);

    expect(
      screen.getByText('You have not selected any roles for this user'),
    ).toBeVisible();
  });
  it('returns the user to the users list when they cancel', async () => {
    const { user } = render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });
  it('calls the save function when saved', async () => {
    const { user } = render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const checkBox = screen.getByRole('checkbox', { name: 'Role 1' });
    await user.click(checkBox);
    const saveButton = screen.getByRole('button', { name: 'Confirm and save' });
    await user.click(saveButton);

    expect(mockSaveUserRoleAssignments).toHaveBeenCalledWith(
      'TEST',
      'test@nhs.net',
      ['role-1'],
    );
  });
});

const mockAssignments = [
  { role: 'role-1', scope: 'site:TEST' },
  {
    role: 'role-3',
    scope: 'site:TEST',
  },
];
