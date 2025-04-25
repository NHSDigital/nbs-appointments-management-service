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
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);
    const searchButton = screen.getByRole('button', { name: 'Continue' });
    await user.click(searchButton);

    expect(
      await screen.findByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeVisible();
  });

  it('shows a validation error when an invalid email address is submitted', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);

    const searchButton = screen.getByRole('button', { name: 'Continue' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter email address',
    });

    await user.type(emailInput, 'invalid@@email@nhs.com');
    await user.click(searchButton);

    expect(
      await screen.findByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeVisible();
  });

  it('trims and lowercases the input', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);

    const searchButton = screen.getByRole('button', { name: 'Continue' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter email address',
    });

    await user.type(emailInput, '   TEST@nhs.net  ');
    await user.click(searchButton);

    expect(
      await screen.queryByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40nhs.net',
    );
  });

  it('takes the user to the main user page when they cancel', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);

    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/users');
  });

  it('initiates the user edit when a valid email address is submitted', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);

    const searchButton = screen.getByRole('button', { name: 'Continue' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter email address',
    });

    await user.type(emailInput, 'test@nhs.net');
    await user.click(searchButton);

    expect(
      await screen.queryByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40nhs.net',
    );
  });

  it('initiates the user edit with lower case when a valid email address is submitted', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={false} />);

    const searchButton = screen.getByRole('button', { name: 'Continue' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter email address',
    });

    await user.type(emailInput, 'TEST@NHS.NET');
    await user.click(searchButton);

    expect(
      await screen.queryByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40nhs.net',
    );
  });

  it('initiates the user edit when okta is enabled and emails is not nhs email', async () => {
    const { user } = render(<FindUserForm site="TEST" oktaEnabled={true} />);

    const searchButton = screen.getByRole('button', { name: 'Continue' });
    const emailInput = screen.getByRole('textbox', {
      name: 'Enter email address',
    });
    const heading = screen.getByRole('heading', {
      level: 2,
      name: 'Add a user',
    });

    await user.type(emailInput, 'test@okta.net');
    await user.click(searchButton);

    expect(
      await screen.queryByText(
        'Email address must be nhs.net or an authorised email domain',
      ),
    ).toBeNull();
    expect(mockReplace).toHaveBeenCalledWith(
      '/site/TEST/users?user=test%40okta.net',
    );
    expect(heading).toBeInTheDocument();
  });
});
