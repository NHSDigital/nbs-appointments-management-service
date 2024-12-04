import { render, screen } from '@testing-library/react';
import NhsPage from './nhs-page';
import {
  fetchUserProfile,
  fetchPermissions,
} from '@services/appointmentsService';
import { UserProfile } from '@types';

jest.mock('@services/appointmentsService');
const fetchUserProfileMock = fetchUserProfile as jest.Mock<
  Promise<UserProfile | undefined>
>;
const fetchPermissionsMock = fetchPermissions as jest.Mock<
  Promise<string[] | undefined>
>;

jest.mock('@components/nhs-header-log-in', () => {
  const MockNhsHeaderLogIn = () => {
    return <button type="submit">log in</button>;
  };
  return MockNhsHeaderLogIn;
});

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
    fetchUserProfileMock.mockResolvedValue({
      emailAddress: 'test@nhs.net',
      availableSites: [
        {
          id: 'TEST',
          name: 'Test site',
          address: '',
        },
      ],
    });
    fetchPermissionsMock.mockResolvedValue([
      'availability:query',
      'availability:set-setup',
      'site:manage',
      'users:view',
    ]);
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  it('shows the correct title', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
    });
    render(jsx);
    expect(screen.getByRole('heading', { name: /Test title/i })).toBeVisible();
  });

  it('shows the correct breadcrumbs including title', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
    });
    render(jsx);
    expect(screen.getByRole('link', { name: 'Level One' })).toBeVisible();
    expect(screen.getByRole('link', { name: 'Level Two' })).toBeVisible();
    expect(screen.getByRole('listitem', { name: /Test title/i })).toBeVisible();
  });

  it('shows the correct breadcrumbs without title', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
      omitTitleFromBreadcrumbs: true,
    });
    render(jsx);
    expect(screen.getByRole('link', { name: 'Level One' })).toBeVisible();
    expect(screen.getByRole('link', { name: 'Level Two' })).toBeVisible();
    expect(screen.queryByRole('listitem', { name: /Test title/i })).toBeNull();
  });

  it('Displays a notification banner when there is an ams-notification cookie', async () => {
    mockGetCookies = jest.fn().mockImplementation((cookieName: string) => {
      return cookieName === 'ams-notification'
        ? { value: 'This is a notification' }
        : undefined;
    });

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
      omitTitleFromBreadcrumbs: true,
    });
    render(jsx);

    expect(screen.getByText('This is a notification')).toBeVisible();
  });

  it('Does not display a notification banner when there is an ams-notification cookie', async () => {
    mockGetCookies = jest.fn().mockReturnValue(undefined);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [
        { name: 'Level One', href: '/' },
        { name: 'Level Two', href: '/' },
      ],
      omitTitleFromBreadcrumbs: true,
    });
    render(jsx);

    expect(screen.queryByText('This is a notification')).toBeNull();
  });

  it('Displays all navigation links if all permissions are present', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: { id: 'TEST', name: 'Test site', address: '' },
      breadcrumbs: [],
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledWith('TEST');

    expect(
      screen.getByRole('navigation', { name: 'Primary navigation' }),
    ).toBeVisible();

    const viewAvailabilityLink = screen.getByRole('link', {
      name: 'View availability',
    });
    const createAvailabilityLink = screen.getByRole('link', {
      name: 'Create availability',
    });
    const manageSiteLink = screen.getByRole('link', {
      name: 'Change site details',
    });
    const viewUsersLink = screen.getByRole('link', { name: 'Manage users' });

    expect(viewAvailabilityLink).toBeVisible();
    expect(viewAvailabilityLink).toHaveAttribute(
      'href',
      '/site/TEST/view-availability',
    );

    expect(createAvailabilityLink).toBeVisible();
    expect(createAvailabilityLink).toHaveAttribute(
      'href',
      '/site/TEST/create-availability',
    );

    expect(manageSiteLink).toBeVisible();
    expect(manageSiteLink).toHaveAttribute('href', '/site/TEST/details');

    expect(viewUsersLink).toBeVisible();
    expect(viewUsersLink).toHaveAttribute('href', '/site/TEST/users');
  });

  it('Does not request permissions if not site is provided', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
    });
    render(jsx);

    expect(fetchPermissionsMock).not.toHaveBeenCalled();
  });

  it('Does not display any navigation links if no permissions are present', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: { id: 'TEST', name: 'Test site', address: '' },
      breadcrumbs: [],
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledWith('TEST');

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
});
