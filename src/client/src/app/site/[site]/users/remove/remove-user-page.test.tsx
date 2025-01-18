import { screen } from '@testing-library/react';
import ConfirmRemoveUserForm from './remove-user-page';
import { useRouter } from 'next/navigation';
import { mockSite } from '@testing/data';
import render from '@testing/render';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockSaveUserRoleAssignments = jest.spyOn(
  appointmentsService,
  'removeUserFromSite',
);

describe('Remove User Page', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders', () => {
    render(<ConfirmRemoveUserForm site={mockSite} user="mock.user@nhs.net" />);

    expect(
      screen.getByText(
        'Are you sure you wish to remove mock.user@nhs.net from Site Alpha?',
      ),
    );
  });

  it('returns the user to the users list when they cancel', async () => {
    const { user } = render(
      <ConfirmRemoveUserForm site={mockSite} user="mock.user@nhs.net" />,
    );

    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith(
      '/site/34e990af-5dc9-43a6-8895-b9123216d699/users',
    );
  });

  it('calls the save function when saved', async () => {
    const { user } = render(
      <ConfirmRemoveUserForm site={mockSite} user="mock.user@nhs.net" />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Remove this account' }),
    );

    expect(mockSaveUserRoleAssignments).toHaveBeenCalledWith(
      '34e990af-5dc9-43a6-8895-b9123216d699',
      'mock.user@nhs.net',
    );
  });
});
