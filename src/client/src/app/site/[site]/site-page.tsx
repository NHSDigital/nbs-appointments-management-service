'use client';
import { Card, SummaryList } from '@nhsuk-frontend-components';
import { mapSiteOverviewSummaryData } from '@services/siteService';
import { Site, WellKnownOdsEntry } from '@types';
import { getAppInsightsClient } from '../../appInsights';
import { useEffect } from 'react';

interface SitePageProps {
  site: Site;
  permissions: string[];
  permissionsAtAnySite: string[];
  wellKnownOdsCodeEntries: WellKnownOdsEntry[];
  siteStatusEnabled: boolean;
}

export const SitePage = ({
  site,
  permissions,
  permissionsAtAnySite,
  wellKnownOdsCodeEntries,
  siteStatusEnabled,
}: SitePageProps) => {
  useEffect(() => {
    const appInsightsClient = getAppInsightsClient();
    appInsightsClient.trackEvent({
      name: 'Site Loaded',
      properties: { name: site.name },
    });
  }, [site.name]);

  const permissionsRelevantToCards = permissions.filter(
    p =>
      p === 'users:view' ||
      p === 'site:manage' ||
      p === 'site:view' ||
      p === 'availability:setup' ||
      p === 'availability:query' ||
      p === 'reports:sitesummary',
  );

  const overviewData = mapSiteOverviewSummaryData(
    site,
    wellKnownOdsCodeEntries,
    siteStatusEnabled,
  );

  return (
    <>
      {overviewData && <SummaryList {...overviewData}></SummaryList>}

      {permissionsRelevantToCards.length > 0 && (
        <ul className="nhsuk-grid-row nhsuk-card-group">
          {permissionsRelevantToCards.includes('availability:query') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`/site/${site.id}/view-availability`}
                title="View availability and manage appointments for your site"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('availability:setup') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`/site/${site.id}/create-availability`}
                title="Create availability"
              />
            </li>
          )}
          {(permissionsRelevantToCards.includes('site:manage') ||
            permissionsRelevantToCards.includes('site:view')) && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`/site/${site.id}/details`}
                title="Change site details and accessibility information"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card href={`/site/${site.id}/users`} title="Manage users" />
            </li>
          )}
          {permissionsAtAnySite.includes('reports:sitesummary') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card href={`/reports`} title="Download reports" />
            </li>
          )}
        </ul>
      )}
    </>
  );
};
