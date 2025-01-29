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
      hasSites: true,
    });
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
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
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
      originPage: '',
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
      originPage: '',
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
      originPage: '',
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
      originPage: '',
    });
    render(jsx);

    expect(screen.queryByText('This is a notification')).toBeNull();
  });

  it('Displays all navigation links if all permissions are present', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: {
        id: '6877d86e-c2df-4def-8508-e1eccf0ea6be',
        name: 'Test site',
        address: '',
        odsCode: 'K12',
        integratedCareBoard: '',
        region: '',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
      },
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledWith(
      '6877d86e-c2df-4def-8508-e1eccf0ea6be',
    );

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
      '/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/view-availability',
    );

    expect(createAvailabilityLink).toBeVisible();
    expect(createAvailabilityLink).toHaveAttribute(
      'href',
      '/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/create-availability',
    );

    expect(manageSiteLink).toBeVisible();
    expect(manageSiteLink).toHaveAttribute(
      'href',
      '/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details',
    );

    expect(viewUsersLink).toBeVisible();
    expect(viewUsersLink).toHaveAttribute(
      'href',
      '/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/users',
    );
  });

  it('Does not request permissions if not site is provided', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(fetchPermissionsMock).not.toHaveBeenCalled();
  });

  it('Does not display any navigation links if no permissions are present', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: {
        id: '6877d86e-c2df-4def-8508-e1eccf0ea6be',
        name: 'Test site',
        address: '',
        odsCode: 'K12',
        integratedCareBoard: '',
        region: '',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
      },
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledWith(
      '6877d86e-c2df-4def-8508-e1eccf0ea6be',
    );

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

  it('Displays a Change Site link if a site is provided', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: {
        id: '6877d86e-c2df-4def-8508-e1eccf0ea6be',
        name: 'Test site',
        address: '',
        odsCode: 'K12',
        integratedCareBoard: '',
        region: '',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
      },
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(
      screen.getByRole('link', { name: 'Change site' }),
    ).toBeInTheDocument();
  });

  it('Does not display a Change Site link if no site is provided', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(screen.queryByRole('link', { name: 'Change site' })).toBeNull();
  });

  it('displays the back link with the correct title and URL', async () => {
    fetchPermissionsMock.mockResolvedValue([]);

    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      site: {
        id: '6877d86e-c2df-4def-8508-e1eccf0ea6be',
        name: 'Test site',
        address: '',
        odsCode: 'K12',
        integratedCareBoard: '',
        region: '',
        phoneNumber: '01189998819991197253',
        location: {
          coordinates: [],
          type: 'point',
        },
      },
      breadcrumbs: [],
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
