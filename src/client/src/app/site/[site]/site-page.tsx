import { Card, SummaryList } from '@nhsuk-frontend-components';
import { mapSiteOverviewSummaryData } from '@services/siteService';
import { Site, WellKnownOdsEntry } from '@types';

interface SitePageProps {
  site: Site;
  permissions: string[];
  wellKnownOdsCodeEntries: WellKnownOdsEntry[];
}

export const SitePage = ({
  site,
  permissions,
  wellKnownOdsCodeEntries,
}: SitePageProps) => {
  const permissionsRelevantToCards = permissions.filter(
    p =>
      p === 'users:view' ||
      p === 'site:manage' ||
      p === 'site:view' ||
      p === 'availability:setup' ||
      p === 'availability:query',
  );

  const overviewData = mapSiteOverviewSummaryData(
    site,
    wellKnownOdsCodeEntries,
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
        </ul>
      )}
    </>
  );
};
