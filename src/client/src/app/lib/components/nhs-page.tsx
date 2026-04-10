'use server';
import { Breadcrumbs, Breadcrumb } from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import NhsNotificationBanner from '@components/notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';
import { Site } from '@types';
import {
  fetchPermissions,
  fetchUserProfile,
} from '@services/appointmentsService';
import BackLink, { NavigationByHrefProps } from './nhsuk-frontend/back-link';
import FeedbackBanner from '@components/feedback-banner';
import BuildNumber from './build-number';
import PrintPageButton from './print-page-button';
import fromServer from '@server/fromServer';
import NhsPageHeader, {
  NavigationLink,
} from './nhsuk-frontend/nhs-page-header';
import { GetCurrentDateTime } from '@services/timeService';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
  site?: Site;
  backLink?: NavigationByHrefProps;
  originPage: string;
  showPrintButton?: boolean;
  secondaryNavigation?: ReactNode;
} & NhsHeadingProps;

const NhsPage = async ({
  title,
  caption,
  site,
  children = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
  backLink,
  originPage,
  showPrintButton = false,
  secondaryNavigation,
}: Props) => {
  const cookieStore = await cookies();
  const notification = cookieStore.get('ams-notification')?.value;
  const navigationLinks = await getLinksForSite(site, originPage);
  const userProfile = await fromServer(fetchUserProfile());

  return (
    <>
      <NhsPageHeader
        navigationLinks={navigationLinks}
        showChangeSiteButton={site !== undefined}
        userEmail={userProfile.emailAddress}
        siteName={site?.name}
      ></NhsPageHeader>
      <FeedbackBanner originPage={originPage} />
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(breadcrumbs.length > 0 && !omitTitleFromBreadcrumbs
            ? [{ name: title ?? '' }]
            : []),
        ]}
      />
      <NhsMainContainer>
        {backLink && (
          <BackLink
            href={backLink.href}
            renderingStrategy={backLink.renderingStrategy}
            text={backLink.text}
          />
        )}

        {secondaryNavigation}

        <div className="nhsuk-grid-row">
          {title && (
            <div className="nhsuk-grid-column-three-quarters">
              <NhsHeading title={title} caption={caption} />
            </div>
          )}
          <div className="nhsuk-grid-column-one-quarter">
            {showPrintButton && (
              <div className="custom-print-button-wrapper nhsuk-u-padding-top-3">
                <PrintPageButton />
              </div>
            )}
          </div>
        </div>

        <NhsNotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter buildNumber={<BuildNumber />} />
    </>
  );
};

const getLinksForSite = async (
  site: Site | undefined,
  originUrl: string,
): Promise<NavigationLink[]> => {
  const [permissionsAtSite, permissionsAtAnySite] = await Promise.all([
    fromServer(fetchPermissions(site?.id)),
    fromServer(fetchPermissions('*')),
  ]);

  const hasAnyReportPermissions = () => {
    return (
      permissionsAtAnySite.includes('reports:sitesummary') ||
      permissionsAtAnySite.includes('reports:siteusers') ||
      permissionsAtAnySite.includes('reports:master-site-list')
    );
  };

  const basePath = process.env.CLIENT_BASE_PATH;

  // Use the originUrl prop which is already reliable
  const encodedReturnUrl = encodeURIComponent(originUrl);

  const navigationLinks: NavigationLink[] = [];

  if (site !== undefined) {
    if (permissionsAtSite.includes('site:view')) {
      navigationLinks.push({
        label: 'Home',
        href: `${basePath}/site/${site.id}`,
        active: {
          type: 'endsWith',
          path: `/site/${site.id}`,
        },
      });
    }

    if (permissionsAtSite.includes('availability:query')) {
      navigationLinks.push({
        label: 'View availability',
        href: `${basePath}/site/${site.id}/view-availability/daily-appointments?date=${GetCurrentDateTime('YYYY-MM-DD')}&page=1`,
        active: {
          type: 'includes',
          path: `/site/${site.id}/view-availability`,
        },
      });
    }

    if (permissionsAtSite.includes('availability:setup')) {
      navigationLinks.push({
        label: 'Create availability',
        href: `${basePath}/site/${site.id}/create-availability`,
        active: {
          type: 'includes',
          path: `/site/${site.id}/create-availability`,
        },
      });
    }

    if (
      permissionsAtSite.includes('site:manage') ||
      permissionsAtSite.includes('site:view')
    ) {
      navigationLinks.push({
        label: 'Change site details',
        href: `${basePath}/site/${site.id}/details`,
        active: {
          type: 'includes',
          path: `/site/${site.id}/details`,
        },
      });
    }

    if (permissionsAtSite.includes('users:view')) {
      navigationLinks.push({
        label: 'Manage users',
        href: `${basePath}/site/${site.id}/users`,
        active: {
          type: 'includes',
          path: `/site/${site.id}/users`,
        },
      });
    }
  }

    if (hasAnyReportPermissions()) {
        navigationLinks.push({
            label: 'Reports',
            // Your logic for the return URL
            href: `${basePath}/reports?returnUrl=${encodedReturnUrl}`,
            // Main's logic for the active link styling
            active: {
                type: 'endsWith',
                path: '/reports',
            },
        });
    }

  return navigationLinks;
};

export default NhsPage;
