'use client';
import { Site, WellKnownOdsEntry } from '@types';
import { getAppInsightsClient } from '../../appInsights';
import { useEffect } from 'react';
import {
  ArrowRightCircleIcon,
  Card,
  SummaryList,
} from 'nhsuk-react-components';
import { mapSiteOverviewSummaryData } from '@services/siteService';

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
      <SummaryList>
        {overviewData?.items.map((item, index) => (
          <SummaryList.Row key={index}>
            <SummaryList.Key>{item.title}</SummaryList.Key>
            <SummaryList.Value>
              {typeof item.value === 'string' ? (
                item.tag !== undefined ? (
                  <span className={`nhsuk-tag nhsuk-tag--${item.tag?.colour}`}>
                    {item.value}
                  </span>
                ) : (
                  item.value
                )
              ) : (
                <ul className="nhsuk-list nhsuk-list--bullet">
                  {item.value?.map((line, lineIndex) => (
                    <li key={lineIndex}>{line}</li>
                  ))}
                </ul>
              )}
            </SummaryList.Value>
          </SummaryList.Row>
        ))}
      </SummaryList>

      {permissionsRelevantToCards.length > 0 && (
        <ul className="nhsuk-grid-row nhsuk-card-group">
          {permissionsRelevantToCards.includes('availability:query') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card primary clickable>
                <Card.Heading headingLevel="h3">
                  <Card.Link
                    href={`/manage-your-appointments/site/${site.id}/view-availability`}
                  >
                    View availability and manage appointments for your site
                  </Card.Link>
                </Card.Heading>
                <ArrowRightCircleIcon />
              </Card>
            </li>
          )}
          {permissionsRelevantToCards.includes('availability:setup') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card primary clickable>
                <Card.Heading headingLevel="h3">
                  <Card.Link href={`/site/${site.id}/create-availability`}>
                    Create availability
                  </Card.Link>
                </Card.Heading>
                <ArrowRightCircleIcon />
              </Card>
            </li>
          )}
          {permissionsRelevantToCards.includes('site:manage') ||
            (permissionsRelevantToCards.includes('site:view') && (
              <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
                <Card clickable>
                  <Card.Heading headingLevel="h3">
                    <Card.Link href={`/site/${site.id}/details`}>
                      Change site details and accessibility information
                    </Card.Link>
                  </Card.Heading>
                  <ArrowRightCircleIcon />
                </Card>
              </li>
            ))}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card primary clickable>
                <Card.Heading headingLevel="h3">
                  <Card.Link href={`/site/${site.id}/users`}>
                    Manage users
                  </Card.Link>
                </Card.Heading>
                <ArrowRightCircleIcon />
              </Card>
            </li>
          )}
          {permissionsAtAnySite.includes('reports:sitesummary') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card primary clickable>
                <Card.Heading headingLevel="h3">
                  <Card.Link href={`/reports`}>Download reports</Card.Link>
                </Card.Heading>
                <ArrowRightCircleIcon />
              </Card>
            </li>
          )}
        </ul>
      )}
    </>
  );
};
