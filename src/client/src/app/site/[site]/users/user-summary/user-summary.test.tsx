import { render, screen, waitFor } from '@testing-library/react';
import UserSummary from './user-summary';
import { useRouter } from 'next/navigation';

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

describe('User Summary Page', () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    (useRouter as jest.Mock).mockReturnValue({ push: mockPush });
    // // Clear previous mocks or session data
    sessionStorage.clear();
    jest.clearAllMocks();
  });

  it.each([[true], [false]])('renders', async (isOkta: boolean) => {
    // Setup sessionStorage mock
    const firstName = isOkta ? 'firstName' : '';
    const lastName = isOkta ? 'lastName' : '';

    sessionStorage.setItem(
      'userFormData',
      JSON.stringify({
        site: '123',
        user: isOkta ? 'test@okta.net' : 'test@nhs.net',
        firstName: firstName,
        lastName: lastName,
        roles: ['Admin', 'Viewer'],
        isEdit: false,
      }),
    );

    render(<UserSummary />);

    const ddElement = screen.getByLabelText('Name-description');

    isOkta
      ? expect(ddElement).toHaveTextContent(firstName + ' ' + lastName)
      : expect(ddElement).toHaveTextContent('');
  });

  it.each([
    [true, true, true, false, true],
    [true, false, false, false, true],
    [false, true, true, true, true],
    [false, false, false, true, true],
  ])(
    'renders change buttons correctly',
    async (
      isEdit: boolean,
      isOkta: boolean,
      changeNameAvailable: boolean,
      changeEmailAvailable: boolean,
      changeRolesAvailable: boolean,
    ) => {
      // Setup sessionStorage mock
      sessionStorage.setItem(
        'userFormData',
        JSON.stringify({
          site: '123',
          user: isOkta ? 'test@okta.net' : 'test@nhs.net',
          firstName: 'firstName',
          lastName: 'lastName',
          roles: ['Admin', 'Viewer'],
          isEdit: isEdit,
        }),
      );

      render(<UserSummary />);

      const nameChangeBtn = screen.queryByLabelText('Name-description-action');
      const emailChangeBtn = screen.queryByLabelText(
        'Email address-description-action',
      );
      const rolesChangeBtn = screen.queryByLabelText(
        'Roles-description-action',
      );

      changeNameAvailable
        ? expect(nameChangeBtn).toBeInTheDocument()
        : expect(nameChangeBtn).not.toBeInTheDocument();

      changeEmailAvailable
        ? expect(emailChangeBtn).toBeInTheDocument()
        : expect(emailChangeBtn).not.toBeInTheDocument();

      changeRolesAvailable
        ? expect(rolesChangeBtn).toBeInTheDocument()
        : expect(rolesChangeBtn).not.toBeInTheDocument();
    },
  );

  it.each([
    [true, true, ''],
    [true, false, ''],
    [false, true, ' will be sent information about how to login.'],
    [false, false, ' will be sent information about how to login.'],
  ])(
    'renders submit message correctly',
    async (isEdit: boolean, isOkta: boolean, expectedMessage: string) => {
      // Setup sessionStorage mock
      sessionStorage.setItem(
        'userFormData',
        JSON.stringify({
          site: '123',
          user: isOkta ? 'test@okta.net' : 'test@nhs.net',
          firstName: 'firstName',
          lastName: 'lastName',
          roles: ['Admin', 'Viewer'],
          isEdit: isEdit,
        }),
      );

      render(<UserSummary />);

      const submitionNote = screen.getByLabelText('submition-note');

      expect(submitionNote).toHaveTextContent(expectedMessage);
    },
  );

  it('redirects if sessionStorage is empty', async () => {
    render(<UserSummary />);

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/');
    });
  });
});
