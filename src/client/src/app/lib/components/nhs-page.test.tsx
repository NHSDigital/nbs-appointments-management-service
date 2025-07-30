import { render, screen } from '@testing-library/react';
import NhsPage from './nhs-page';
import {
  fetchUserProfile,
  fetchPermissions,
  fetchFeatureFlag,
} from '@services/appointmentsService';
import { FeatureFlag, UserProfile } from '@types';
import { mockAllPermissions, mockSite } from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchUserProfileMock = fetchUserProfile as jest.Mock<
  Promise<UserProfile | undefined>
>;
const fetchPermissionsMock = fetchPermissions as jest.Mock<
  Promise<string[] | undefined>
>;

const fetchFeatureFlagMock = fetchFeatureFlag as jest.Mock<
  Promise<FeatureFlag | undefined>
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

    fetchFeatureFlagMock.mockResolvedValue({
      enabled: true,
    });
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
        { name: 'Level One', href: '/sites' },
        { name: 'Level Two', href: '/sites' },
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
        { name: 'Level One', href: '/sites' },
        { name: 'Level Two', href: '/sites' },
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
        { name: 'Level One', href: '/sites' },
        { name: 'Level Two', href: '/sites' },
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
        { name: 'Level One', href: '/sites' },
        { name: 'Level Two', href: '/sites' },
      ],
      omitTitleFromBreadcrumbs: true,
      originPage: '',
    });
    render(jsx);

    expect(screen.queryByText('This is a notification')).toBeNull();
  });

  it.each([
    ['availability:query', 'View availability', 'view-availability'],
    ['availability:setup', 'Create availability', 'create-availability'],
    ['site:manage', 'Change site details', 'details'],
    ['site:view', 'Change site details', 'details'],
    ['users:view', 'Manage users', 'users'],
    ['reports:sitesummary', 'Reports', 'reports'],
  ])(
    'displays the correct cards when permissions are present',
    async (permission: string, cardTitle: string, path: string) => {
      fetchPermissionsMock.mockResolvedValue([permission]);

      const jsx = await NhsPage({
        title: 'Test title',
        children: null,
        site: mockSite,
        breadcrumbs: [],
        originPage: '',
      });
      render(jsx);

      expect(fetchPermissionsMock).toHaveBeenCalledWith(
        '34e990af-5dc9-43a6-8895-b9123216d699',
      );

      expect(fetchFeatureFlagMock).toHaveBeenCalledWith('SiteSummaryReport');

      expect(screen.getByRole('link', { name: cardTitle })).toBeInTheDocument();
      if (path === 'reports') {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/${path}`,
        );
      } else {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/site/${mockSite.id}/${path}`,
        );
      }
    },
  );

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
      site: mockSite,
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledWith(
      '34e990af-5dc9-43a6-8895-b9123216d699',
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

  it.each([
    [
      ['availability:query'],
      'View availability and manage appointments for your site',
    ],
    [['availability:setup'], 'Create availability'],
    [
      ['site:manage', 'site:view'],
      'Change site details and accessibility information',
    ],
    [['users:view'], 'Manage users'],
    [['reports:sitesummary'], 'Download reports'],
  ])(
    'hides the correct links when permissions are lacking',
    async (permissions: string[], cardTitle: string) => {
      fetchPermissionsMock.mockResolvedValue(
        mockAllPermissions.filter(p => !permissions.includes(p)),
      );

      const jsx = await NhsPage({
        title: 'Test title',
        children: null,
        site: mockSite,
        breadcrumbs: [],
        originPage: '',
      });
      render(jsx);

      expect(fetchPermissionsMock).toHaveBeenCalledWith(
        '34e990af-5dc9-43a6-8895-b9123216d699',
      );

      expect(
        screen.queryByRole('link', { name: cardTitle }),
      ).not.toBeInTheDocument();
    },
  );

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
        accessibilities: [],
        informationForCitizens: '',
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
        accessibilities: [],
        informationForCitizens: '',
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
