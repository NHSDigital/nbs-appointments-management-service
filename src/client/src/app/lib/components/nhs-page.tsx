'use server';
import {
  Breadcrumbs,
  Breadcrumb,
  Header,
  NavigationLink,
} from '@nhsuk-frontend-components';
import { ReactNode } from 'react';
import NotificationBanner from '@components/notification-banner';
import { cookies } from 'next/headers';
import NhsFooter from '@components/nhs-footer';
import NhsMainContainer from '@components/nhs-main-container';
import NhsHeaderLogOut from '@components/nhs-header-log-out';
import NhsHeading, { NhsHeadingProps } from './nhs-heading';
import { Site } from '@types';
import { fetchPermissions } from '@services/appointmentsService';

type Props = {
  children: ReactNode;
  headerAuthComponent?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  omitTitleFromBreadcrumbs?: boolean;
  site?: Site;
} & NhsHeadingProps;

const NhsPage = async ({
  title,
  caption,
  site,
  children = null,
  headerAuthComponent = null,
  breadcrumbs = [],
  omitTitleFromBreadcrumbs,
}: Props) => {
  const notification = cookies().get('ams-notification')?.value;
  const navigationLinks = await getLinksForSite(site);

  return (
    <>
      <Header
        navigationLinks={navigationLinks}
        showChangeSiteButton={site !== undefined}
      >
        {headerAuthComponent ?? NhsHeaderLogOut()}
      </Header>
      <Breadcrumbs
        trail={[
          ...breadcrumbs,
          ...(!omitTitleFromBreadcrumbs ? [{ name: title }] : []),
        ]}
      />
      <NhsMainContainer>
        <NhsHeading title={title} caption={caption} />
        <NotificationBanner notification={notification} />
        {children}
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
};

const getLinksForSite = async (
  site: Site | undefined,
): Promise<NavigationLink[]> => {
  if (site === undefined) {
    return [];
  }

  const permissions = await fetchPermissions(site.id);

  const navigationLinks: NavigationLink[] = [];

  if (permissions.includes('availability:query')) {
    navigationLinks.push({
      label: 'View availability',
      href: `/site/${site.id}/view-availability`,
    });
  }

  if (permissions.includes('availability:set-setup')) {
    navigationLinks.push({
      label: 'Create availability',
      href: `/site/${site.id}/create-availability`,
    });
  }

  if (permissions.includes('site:manage')) {
    navigationLinks.push({
      label: 'Change site details',
      href: `/site/${site.id}/details`,
    });
  }

  if (permissions.includes('users:view')) {
    navigationLinks.push({
      label: 'Manage users',
      href: `/site/${site.id}/users`,
    });
  }

  return navigationLinks;
};

export default NhsPage;
