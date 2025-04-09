import { screen } from '@testing-library/react';
import UserDetailsForm from './user-details-form';
import { RoleAssignment } from '@types';
import { useRouter } from 'next/navigation';
import { mockAssignments, mockRoles } from '@testing/data';
import render from '@testing/render';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();
const mockPush = jest.fn();

jest.mock('@services/appointmentsService');
const mockSaveUserRoleAssignments = jest.spyOn(
  appointmentsService,
  'saveUserRoleAssignments',
);

jest.spyOn(window.sessionStorage.__proto__, 'setItem');
window.sessionStorage.setItem = jest.fn();

describe('Assign Roles Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
      push: mockPush,
    });
  });

  it.skip('displays a check box for each available role', () => {
    render(
      <UserDetailsForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );

    expect(screen.getByRole('checkbox', { name: 'Alpha Role' })).toBeVisible();
    expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeVisible();
    expect(
      screen.getByRole('checkbox', { name: 'Charlie Role' }),
    ).toBeVisible();
  });

  it('displays checkboxes for all available roles in ascending alphabetical order', () => {
    render(
      <UserDetailsForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );
    const checkboxes = screen.getAllByRole('checkbox');
    expect(checkboxes.length).toBe(3);
    expect(checkboxes[0].getAttribute('label')).toEqual('Alpha Role');
    expect(checkboxes[1].getAttribute('label')).toEqual('Beta Role');
    expect(checkboxes[2].getAttribute('label')).toEqual('Charlie Role');
  });

  it('checks the correct options', () => {
    render(
      <UserDetailsForm
        site="TEST"
        user="test@nhs.net"
        assignments={mockAssignments}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );

    expect(screen.getByRole('checkbox', { name: 'Beta Role' })).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Charlie Role' }),
    ).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Alpha Role' })).toBeChecked();
  });
  it('display a validation error when attempting to submit the form with no roles selected', async () => {
    const { user } = render(
      <UserDetailsForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );
    const submitButton = screen.getByRole('button', {
      name: 'Continue',
    });
    await user.click(submitButton);

    expect(
      screen.getByText('You have not selected any roles for this user'),
    ).toBeVisible();
  });

  it('returns the user to the users list when they cancel', async () => {
    const { user } = render(
      <UserDetailsForm
        site="TEST"
        user="test@nhs.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );
    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });

  it('calls user summary page', async () => {
    const { user } = render(
      <UserDetailsForm
        site="TEST"
        user="test@okta.net"
        assignments={[] as RoleAssignment[]}
        roles={mockRoles}
        firstName="firstName"
        lastName="lastName"
        isEdit={false}
      />,
    );
    const checkBox = screen.getByRole('checkbox', { name: 'Beta Role' });
    await user.click(checkBox);
    const continueButton = screen.getByRole('button', { name: 'Continue' });
    await user.click(continueButton);

    expect(sessionStorage.setItem).toHaveBeenCalledWith(
      'userFormData',
      JSON.stringify({
        site: 'TEST',
        user: 'test@okta.net',
        roles: ['role-1'],
        firstName: 'firstName',
        lastName: 'lastName',
        isEdit: false,
      }),
    );
    expect(mockPush).toHaveBeenCalledWith('/site/TEST/users/user-summary');
  });
});
