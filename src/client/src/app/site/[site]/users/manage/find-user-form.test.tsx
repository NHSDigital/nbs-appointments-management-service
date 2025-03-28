import { screen } from '@testing-library/react';
import FindUserForm from './find-user-form';
import { usePathname, useRouter } from 'next/navigation';
import render from '@testing/render';

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
    const { user } = render(<FindUserForm site="TEST" />);
    const searchButton = screen.getByRole('button', { name: 'Search user' });
    await user.click(searchButton);

    expect(
      await screen.findByText('You have not entered a valid NHS email address'),
    ).toBeVisible();
  });

  it('shows a validation error when an invalid email address is submitted', async () => {
    const { user } = render(<FindUserForm site="TEST" />);

    const searchButton = screen.getByRole('button', { name: 'Search user' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter an email address',
    });

    await user.type(emailInput, 'invalid@@email@nhs.com');
    await user.click(searchButton);

    expect(
      await screen.findByText('You have not entered a valid NHS email address'),
    ).toBeVisible();
  });

  it('trims and lowercases the input', async () => {
    const { user } = render(<FindUserForm site="TEST" />);

    const searchButton = screen.getByRole('button', { name: 'Search user' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter an email address',
    });

    await user.type(emailInput, '   TEST@nhs.net  ');
    await user.click(searchButton);

    expect(
      await screen.queryByText(
        'You have not entered a valid nhs email address',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40nhs.net',
    );
  });

  it('takes the user to the main user page when they cancel', async () => {
    const { user } = render(<FindUserForm site="TEST" />);

    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });

  it('initiates the user edit when a valid email address is submitted', async () => {
    const { user } = render(<FindUserForm site="TEST" />);

    const searchButton = screen.getByRole('button', { name: 'Search user' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter an email address',
    });

    await user.type(emailInput, 'test@nhs.net');
    await user.click(searchButton);

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
