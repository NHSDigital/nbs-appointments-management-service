import { render, screen } from '@testing-library/react';
import { fetchPermissions } from '@services/appointmentsService';
import { mockAllPermissions } from '@testing/data';
import NhsTransactionalPage from './nhs-transactional-page';

jest.mock('@services/appointmentsService');
const fetchPermissionsMock = fetchPermissions as jest.Mock<
  Promise<string[] | undefined>
>;

jest.mock('@components/nhs-header-log-out', () => {
  const MockNhsHeaderLogOut = () => {
    return <button type="submit">log out</button>;
  };
  return MockNhsHeaderLogOut;
});

let mockGetCookies = jest.fn().mockImplementation((cookieName: string) => {
  return cookieName === 'ams-notification'
    ? { value: 'This is a notification' }
    : undefined;
});

jest.mock('next/headers', () => {
  return {
    cookies: () => {
      return {
        get: mockGetCookies,
      };
    },
  };
});

jest.mock('@components/close-notification-form', () => {
  const MockCloseNotificationButton = () => {
    return (
      <button
        type="button"
        className="nhsuk-warning-callout-custom__close-button"
      >
        Close
      </button>
    );
  };
  return MockCloseNotificationButton;
});

describe('Nhs Page', () => {
  beforeEach(() => {
    fetchPermissionsMock.mockResolvedValue([
      'availability:query',
      'availability:setup',
      'site:manage',
      'users:view',
    ]);
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  it('shows the correct title', async () => {
    const jsx = await NhsTransactionalPage({
      title: 'Test title',
      children: null,
      originPage: '',
    });
    render(jsx);
    expect(screen.getByRole('heading', { name: /Test title/i })).toBeVisible();
  });

  it('Displays a notification banner when there is an ams-notification cookie', async () => {
    mockGetCookies = jest.fn().mockImplementation((cookieName: string) => {
      return cookieName === 'ams-notification'
        ? { value: 'This is a notification' }
        : undefined;
    });

    const jsx = await NhsTransactionalPage({
      title: 'Test title',
      children: null,
      originPage: '',
    });
    render(jsx);

    expect(screen.getByText('This is a notification')).toBeVisible();
  });

  it('Does not display a notification banner when there is not an ams-notification cookie', async () => {
    mockGetCookies = jest.fn().mockReturnValue(undefined);

    const jsx = await NhsTransactionalPage({
      title: 'Test title',
      children: null,
      originPage: '',
    });
    render(jsx);

    expect(screen.queryByText('This is a notification')).toBeNull();
  });

  it('Does not display any navigation links even if all permissions are present', async () => {
    fetchPermissionsMock.mockResolvedValue(mockAllPermissions);

    const jsx = await NhsTransactionalPage({
      title: 'Test title',
      children: null,
      originPage: '',
    });
    render(jsx);

    expect(
      screen.queryByRole('navigation', { name: 'Primary navigation' }),
    ).toBeNull();

    expect(
      screen.queryByRole('link', { name: 'View availability' }),
    ).toBeNull();
    expect(
      screen.queryByRole('link', { name: 'Create availability' }),
    ).toBeNull();
    expect(
      screen.queryByRole('link', { name: 'Change site details' }),
    ).toBeNull();
    expect(screen.queryByRole('link', { name: 'Manage users' })).toBeNull();
  });

  it('displays the back link with the correct title and URL', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsTransactionalPage({
      title: 'Test title',
      children: null,
      backLink: {
        href: '/test/url',
        renderingStrategy: 'server',
        text: 'Test back link',
      },
      originPage: '',
    });

    render(jsx);
    expect(
      screen.getByRole('link', { name: 'Test back link' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Test back link' }),
    ).toHaveAttribute('href', '/test/url');
  });
});
