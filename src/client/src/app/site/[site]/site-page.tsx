import { Card, SummaryList, SummaryListItem } from '@nhsuk-frontend-components';
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
      p === 'availability:set-setup' ||
      p === 'availability:query',
  );

  const summaryData = mapSummaryData(site, wellKnownOdsCodeEntries);

  return (
    <>
      {summaryData && <SummaryList {...summaryData}></SummaryList>}
      {permissionsRelevantToCards.length > 0 && (
        <ul className="nhsuk-grid-row nhsuk-card-group">
          {permissionsRelevantToCards.includes('availability:query') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/view-availability`}
                title="View availability and manage appointments for your site"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('availability:set-setup') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/create-availability`}
                title="Create availability"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card href={`${site.id}/users`} title="Manage users" />
            </li>
          )}
          {(permissionsRelevantToCards.includes('site:manage') ||
            permissionsRelevantToCards.includes('site:view')) && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/details`}
                title="Change site details and accessibility information"
              />
            </li>
          )}
        </ul>
      )}
    </>
  );
};

const mapSummaryData = (
  site: Site,
  wellKnownOdsCodeEntries: WellKnownOdsEntry[],
) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  items.push({
    title: 'Address',
    value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
  });
  items.push({ title: 'ODS code', value: site.id });
  items.push({
    title: 'ICB',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.integratedCareBoard)
        ?.displayName ?? site.integratedCareBoard,
  });
  items.push({
    title: 'Region',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.region)
        ?.displayName ?? site.region,
  });

  const border = false;

  return { items, border };
};
