import { render, screen } from '@testing-library/react';
import NhsPage from './nhs-page';
import {
  fetchUserProfile,
  fetchPermissions,
} from '@services/appointmentsService';
import { ServerActionResult, UserProfile } from '@types';
import {
  mockAllPermissions,
  mockSecondaryLinks,
  mockSite,
} from '@testing/data';
import asServerActionResult from '@testing/asServerActionResult';
import { GetCurrentDateTime } from '@services/timeService';
import { SecondaryNavigation } from './secondary-navigation';

jest.mock('@services/appointmentsService');
const fetchUserProfileMock = fetchUserProfile as jest.Mock<
  Promise<ServerActionResult<UserProfile>>
>;
const fetchPermissionsMock = fetchPermissions as jest.Mock<
  Promise<ServerActionResult<string[]>>
>;

const mockUsePathname = jest.fn();
jest.mock('next/navigation', () => {
  return {
    usePathname: () => mockUsePathname(),
  };
});

jest.mock('@components/nhs-header-log-in', () => {
  const MockNhsHeaderLogIn = () => {
    return <button type="submit">log in</button>;
  };
  return MockNhsHeaderLogIn;
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

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    GetCurrentDateTime: jest.fn(),
  };
});
const mockGetCurrentDatTime = GetCurrentDateTime as jest.Mock<string>;

describe('Nhs Page', () => {
  beforeEach(() => {
    fetchUserProfileMock.mockResolvedValue(
      asServerActionResult({
        emailAddress: 'test@nhs.net',
        hasSites: true,
      }),
    );
    fetchPermissionsMock.mockResolvedValue(
      asServerActionResult([
        'availability:query',
        'availability:setup',
        'site:manage',
        'users:view',
      ]),
    );
    mockUsePathname.mockReturnValue('/test/current/path');
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

  it('displays the print button', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
      showPrintButton: true,
    });
    render(jsx);

    expect(
      screen.getByRole('button', { name: 'Print page' }),
    ).toBeInTheDocument();
  });

  it('hides the print button by default', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(
      screen.queryByRole('button', { name: 'Print page' }),
    ).not.toBeInTheDocument();
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
    [
      'availability:query',
      'View availability',
      'view-availability/daily-appointments?date=2026-06-05&page=1',
    ],
    ['availability:setup', 'Create availability', 'create-availability'],
    ['site:manage', 'Change site details', 'details'],
    ['site:view', 'Change site details', 'details'],
    ['users:view', 'Manage users', 'users'],
    ['reports:sitesummary', 'Reports', 'reports'],
    ['reports:siteusers', 'Reports', 'reports'],
    ['reports:master-site-list', 'Reports', 'reports'],
  ])(
    'displays the correct cards when permissions are present',
    async (permission: string, cardTitle: string, path: string) => {
      fetchPermissionsMock.mockResolvedValue(
        asServerActionResult([permission]),
      );
      mockGetCurrentDatTime.mockReturnValue('2026-06-05');

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

      expect(screen.getByRole('link', { name: cardTitle })).toBeInTheDocument();
      if (path === 'reports') {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/manage-your-appointments/${path}`,
        );
      } else {
        expect(screen.getByRole('link', { name: cardTitle })).toHaveAttribute(
          'href',
          `/manage-your-appointments/site/${mockSite.id}/${path}`,
        );
      }
    },
  );

  it('Still requests global permissions if site is not provided', async () => {
    const jsx = await NhsPage({
      title: 'Test title',
      children: null,
      breadcrumbs: [],
      originPage: '',
    });
    render(jsx);

    expect(fetchPermissionsMock).toHaveBeenCalledTimes(2);
    expect(fetchPermissionsMock).toHaveBeenCalledWith(undefined);
    expect(fetchPermissionsMock).toHaveBeenCalledWith('*');
  });

  it('Does not display any navigation links if no permissions are present', async () => {
    fetchPermissionsMock.mockResolvedValue(asServerActionResult([]));

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

    expect(screen.queryByRole('link', { name: 'Home' })).toBeNull();
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
        asServerActionResult(
          mockAllPermissions.filter(p => !permissions.includes(p)),
        ),
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
    fetchPermissionsMock.mockResolvedValue(asServerActionResult([]));

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
          coordinates: [-2.3, 53.1],
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
    fetchPermissionsMock.mockResolvedValue(asServerActionResult([]));

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
    fetchPermissionsMock.mockResolvedValue(asServerActionResult([]));

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
          coordinates: [-2.3, 53.1],
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

  it('displays secondary navigation with the correct details when provided', async () => {
    fetchPermissionsMock.mockResolvedValue(asServerActionResult([]));

    const secondaryNavJsx = await SecondaryNavigation({
      links: mockSecondaryLinks,
    });

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
          coordinates: [-2.3, 53.1],
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
      secondaryNavigation: secondaryNavJsx,
    });

    render(jsx);

    expect(screen.getByRole('link', { name: 'Link One' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link One' })).toHaveAttribute(
      'href',
      '/link/one/url',
    );
    expect(screen.getByRole('link', { name: 'Link Two' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link Two' })).toHaveAttribute(
      'href',
      '/link/two/url',
    );
    expect(screen.getByRole('link', { name: 'Link Two' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(
      screen.getByRole('link', { name: 'Link Three' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Link Three' })).toHaveAttribute(
      'href',
      '/link/three/url',
    );
  });

  it('displays site navigation links when site is defined and correct sets aria current attribute', async () => {
    fetchPermissionsMock.mockResolvedValue(
      asServerActionResult([
        'availability:query',
        'availability:setup',
        'site:manage',
        'users:view',
        'site:view',
      ]),
    );
    mockGetCurrentDatTime.mockReturnValue('2026-06-05');
    mockUsePathname.mockReturnValue(`/site/${mockSite.id}`);

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

    expect(screen.queryByRole('link', { name: 'Home' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Home' })).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}`,
    );
    expect(screen.queryByRole('link', { name: 'Home' })).toHaveAttribute(
      'aria-current',
      'true',
    );

    expect(
      screen.queryByRole('link', { name: 'View availability' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'View availability' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/view-availability/daily-appointments?date=2026-06-05&page=1`,
    );
    expect(
      screen.queryByRole('link', { name: 'View availability' }),
    ).not.toHaveAttribute('aria-current', 'true');

    expect(
      screen.queryByRole('link', { name: 'Create availability' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Create availability' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/create-availability`,
    );
    expect(
      screen.queryByRole('link', { name: 'Create availability' }),
    ).not.toHaveAttribute('aria-current', 'true');

    expect(
      screen.queryByRole('link', { name: 'Change site details' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Change site details' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/details`,
    );
    expect(
      screen.queryByRole('link', { name: 'Change site details' }),
    ).not.toHaveAttribute('aria-current', 'true');

    expect(
      screen.queryByRole('link', { name: 'Manage users' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Manage users' }),
    ).toHaveAttribute(
      'href',
      `/manage-your-appointments/site/${mockSite.id}/users`,
    );
    expect(
      screen.queryByRole('link', { name: 'Manage users' }),
    ).not.toHaveAttribute('aria-current', 'true');
  });
});
