'use server';
import {
  Breadcrumbs,
  Breadcrumb,
  NavigationLink,
} from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import NotificationBanner from '@components/notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';
import { Site } from '@types';
import {
  fetchFeatureFlag,
  fetchPermissions,
  fetchUserProfile,
} from '@services/appointmentsService';
import BackLink, { NavigationByHrefProps } from './nhsuk-frontend/back-link';
import FeedbackBanner from '@components/feedback-banner';
import BuildNumber from './build-number';
import PrintPageButton from './print-page-button';
import fromServer from '@server/fromServer';
import NhsPageHeader from './nhsuk-frontend/nhs-page-header';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
  site?: Site;
  backLink?: NavigationByHrefProps;
  originPage: string;
  showPrintButton?: boolean;
} & NhsHeadingProps;

const NhsPage = async ({
  title,
  caption,
  site,
  children = null,
  headerAuthComponent = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
  backLink,
  originPage,
  showPrintButton = false,
}: Props) => {
  const cookieStore = await cookies();
  const notification = cookieStore.get('ams-notification')?.value;
  const navigationLinks = await getLinksForSite(site);
  const userProfile = await fromServer(fetchUserProfile());

  return (
    <>
      <NhsPageHeader
        navigationLinks={navigationLinks}
        showChangeSiteButton={site !== undefined}
        userEmail={userProfile.emailAddress}
      ></NhsPageHeader>
      <FeedbackBanner originPage={originPage} />
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(breadcrumbs.length > 0 && !omitTitleFromBreadcrumbs
            ? [{ name: title }]
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

        <div className="nhsuk-grid-row">
          <div className="nhsuk-grid-column-three-quarters">
            <NhsHeading title={title} caption={caption} />
          </div>
          <div className="nhsuk-grid-column-one-quarter">
            {showPrintButton && (
              <div className="custom-print-button-wrapper nhsuk-u-padding-top-3">
                <PrintPageButton />
              </div>
            )}
          </div>
        </div>

        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter buildNumber={<BuildNumber />} />
    </>
  );
};

const getLinksForSite = async (
  site: Site | undefined,
): Promise<NavigationLink[]> => {
  const [permissionsAtSite, permissionsAtAnySite, siteSummaryFlag] =
    await Promise.all([
      fromServer(fetchPermissions(site?.id)),
      fromServer(fetchPermissions('*')),
      fromServer(fetchFeatureFlag('SiteSummaryReport')),
    ]);

  const hasAnyReportPermissions = () => {
    return (
      permissionsAtAnySite.includes('reports:sitesummary') ||
      permissionsAtAnySite.includes('reports:siteusers') ||
      permissionsAtAnySite.includes('reports:master-site-list')
    );
  };

  const navigationLinks: NavigationLink[] = [];

  if (site !== undefined) {
    if (permissionsAtSite.includes('availability:query')) {
      navigationLinks.push({
        label: 'View availability',
        href: `/site/${site.id}/view-availability`,
      });
    }

    if (permissionsAtSite.includes('availability:setup')) {
      navigationLinks.push({
        label: 'Create availability',
        href: `/site/${site.id}/create-availability`,
      });
    }

    if (
      permissionsAtSite.includes('site:manage') ||
      permissionsAtSite.includes('site:view')
    ) {
      navigationLinks.push({
        label: 'Change site details',
        href: `/site/${site.id}/details`,
      });
    }

    if (permissionsAtSite.includes('users:view')) {
      navigationLinks.push({
        label: 'Manage users',
        href: `/site/${site.id}/users`,
      });
    }
  }

  if (hasAnyReportPermissions() && siteSummaryFlag.enabled) {
    navigationLinks.push({
      label: 'Reports',
      href: `/reports`,
    });
  }

  return navigationLinks;
};

export default NhsPage;
