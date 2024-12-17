import { screen } from '@testing-library/react';
import AssignRolesForm from './assign-roles-form';
import { RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import { mockAssignments, mockRoles } from '@testing/data';
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

  it.skip('displays a check box for each available role', () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );

    expect(screen.getByRole('checkbox', { name: 'Alfa Role' })).toBeVisible();
    expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeVisible();
    expect(
      screen.getByRole('checkbox', { name: 'Charlie Role' }),
    ).toBeVisible();
  });

  it('displays checkboxes for all available roles in ascending alphabetical order', () => {
    render(
      <AssignRolesForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
      />,
    );
    const checkboxes = screen.getAllByRole('checkbox');
    expect(checkboxes.length).toBe(3);
    expect(checkboxes[0].getAttribute('label')).toEqual('Alfa Role');
    expect(checkboxes[1].getAttribute('label')).toEqual('Beta Role');
    expect(checkboxes[2].getAttribute('label')).toEqual('Charlie Role');
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

    expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Charlie Role' }),
    ).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Alfa Role' })).toBeChecked();
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
    const checkBox = screen.getByRole('checkbox', { name: 'Beta Role' });
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
