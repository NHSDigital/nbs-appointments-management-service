import {
  Card,
  SummaryList,
  SummaryListItem,
  Tabs,
  TabsChildren,
} from '@nhsuk-frontend-components';
import { Site } from '@types';

interface SitePageProps {
  site: Site;
  permissions: string[];
  activeTab?: string;
}

export const SitePage = ({ site, permissions, activeTab }: SitePageProps) => {
  const permissionsRelevantToCards = permissions.filter(
    p =>
      p === 'users:view' ||
      p === 'site:manage' ||
      p === 'site:view' ||
      p === 'availability:set-setup' ||
      p === 'availability:query',
  );

  const summaryData = mapSummaryData(site);
  const tabsChildren = buildTabs(site.id, activeTab);

  return (
    <>
      {summaryData && <SummaryList {...summaryData}></SummaryList>}

      <Tabs>{tabsChildren}</Tabs>

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
          {(permissionsRelevantToCards.includes('site:manage') ||
            permissionsRelevantToCards.includes('site:view')) && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card
                href={`${site.id}/details`}
                title="Change site details and accessibility information"
              />
            </li>
          )}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card href={`${site.id}/users`} title="Manage users" />
            </li>
          )}
        </ul>
      )}
    </>
  );
};

const mapSummaryData = (site: Site) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  items.push({
    title: 'Address',
    value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
  });
  items.push({ title: 'ODS code', value: site.id });
  items.push({ title: 'ICB', value: site.integratedCareBoard });
  items.push({ title: 'Region', value: site.region });

  const border = false;

  return { items, border };
};

const buildTabs = (siteId: string, activeTab?: string): TabsChildren[] => {
  const tabs = [
    {
      isSelected: activeTab === 'title1',
      url: `/site/${siteId}?tab=title1`,
      tabTitle: 'title1',
      content: 'test1',
    },
    {
      isSelected: activeTab === 'title2',
      url: `/site/${siteId}?tab=title2`,
      tabTitle: 'title2',
      content: 'test2',
    },
  ];

  if (!tabs.some(t => t.isSelected)) {
    tabs[0].isSelected = true;
  }

  return tabs;
};
