import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import FindUserForm from './find-user-form';
import { usePathname, useRouter } from 'next/navigation';

jest.mock('next/navigation');

const mockUsePathName = usePathname as jest.Mock<string>;
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

describe('FindUserForm', () => {
  beforeEach(() => {
    mockUsePathName.mockReturnValue('/site/TEST/users');
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });
  it('shows a validation error when no data is submitted', async () => {
    render(<FindUserForm site="TEST" />);
    const searchButton = screen.getByRole('button', { name: 'Search user' });
    await userEvent.click(searchButton);

    expect(
      await screen.findByText('You have not entered a valid NHS email address'),
    ).toBeVisible();
  });

  it('shows a validation error when an none nhs.net email address is submitted', async () => {
    render(<FindUserForm site="TEST" />);

    const searchButton = screen.getByRole('button', { name: 'Search user' });
    const emailInput = screen.getByRole('textbox', {
      name: 'enter an email address',
    });

    await userEvent.type(emailInput, 'test@test.com');
    await userEvent.click(searchButton);

    expect(
      await screen.findByText('You have not entered a valid NHS email address'),
    ).toBeVisible();
  });

  it('takes the user to the main user page when they cancel', async () => {
    render(<FindUserForm site="TEST" />);

    const cancelButton = screen.getByRole('button', { name: 'cancel' });
    await userEvent.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });

  it('initiates the user edit when a valid email address is submitted', async () => {
    render(<FindUserForm site="TEST" />);

    const searchButton = screen.getByRole('button', { name: 'Search user' });
    const emailInput = screen.getByRole('textbox', {
      name: 'enter an email address',
    });

    await userEvent.type(emailInput, 'test@nhs.net');
    await userEvent.click(searchButton);

    expect(
      await screen.queryByText(
        'You have not entered a valid nhs email address',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40nhs.net',
    );
  });
});
