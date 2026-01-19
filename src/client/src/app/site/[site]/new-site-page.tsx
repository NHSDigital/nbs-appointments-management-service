'use client';
import { Site, WellKnownOdsEntry } from '@types';
import { useEffect } from 'react';
import { getAppInsightsClient } from '../../appInsights';
import {
  ArrowRightCircleIcon,
  Card,
  SummaryList,
} from 'nhsuk-react-components';

interface SitePageProps {
  site: Site;
  permissions: string[];
  permissionsAtAnySite: string[];
  wellKnownOdsCodeEntries: WellKnownOdsEntry[];
  siteSummaryEnabled: boolean;
  siteStatusEnabled: boolean;
}

export const NewSitePage = ({
  site,
  permissions,
  permissionsAtAnySite,
  wellKnownOdsCodeEntries,
  siteSummaryEnabled,
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

  return (
    <>
      <SummaryList>
        <SummaryList.Row>
          <SummaryList.Key>Address</SummaryList.Key>
          <SummaryList.Value>
            {site.address.match(/[^,]+,|[^,]+$/g) || []}
          </SummaryList.Value>
        </SummaryList.Row>
        <SummaryList.Row>
          <SummaryList.Key>Phone Number</SummaryList.Key>
          <SummaryList.Value>{site.phoneNumber}</SummaryList.Value>
        </SummaryList.Row>
        <SummaryList.Row>
          <SummaryList.Key>ODS code</SummaryList.Key>
          <SummaryList.Value>{site.odsCode}</SummaryList.Value>
        </SummaryList.Row>
        <SummaryList.Row>
          <SummaryList.Key>ICB</SummaryList.Key>
          <SummaryList.Value>{site.integratedCareBoard}</SummaryList.Value>
        </SummaryList.Row>
        <SummaryList.Row>
          <SummaryList.Key>Region</SummaryList.Key>
          <SummaryList.Value>
            {wellKnownOdsCodeEntries.find(
              e => e.type === 'region' && e.odsCode === site.region,
            )?.displayName ?? site.region}
          </SummaryList.Value>
        </SummaryList.Row>
      </SummaryList>

      {permissionsRelevantToCards.length > 0 && (
        <ul className="nhsuk-grid-row nhsuk-card-group">
          {permissionsRelevantToCards.includes('availability:query') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card cardType="primary" clickable>
                <Card.Content>
                  <Card.Heading className="nhsuk-card__heading nhsuk-heading-m">
                    <Card.Link
                      href={`/manage-your-appointments/site/${site.id}/view-availability`}
                    >
                      View availability and manage appointments for your site
                    </Card.Link>
                  </Card.Heading>
                  <ArrowRightCircleIcon />
                </Card.Content>
              </Card>
            </li>
          )}
          {permissionsRelevantToCards.includes('availability:setup') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card cardType="primary" clickable>
                <Card.Content>
                  <Card.Heading className="nhsuk-card__heading nhsuk-heading-m">
                    <Card.Link
                      href={`/manage-your-appointments/site/${site.id}/create-availability`}
                    >
                      Create availability
                    </Card.Link>
                  </Card.Heading>
                  <ArrowRightCircleIcon />
                </Card.Content>
              </Card>
            </li>
          )}
          {permissionsRelevantToCards.includes('site:manage') ||
            (permissionsRelevantToCards.includes('site:view') && (
              <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
                <Card cardType="primary" clickable>
                  <Card.Content>
                    <Card.Heading className="nhsuk-card__heading nhsuk-heading-m">
                      <Card.Link
                        href={`/manage-your-appointments/site/${site.id}/details`}
                      >
                        Change site details and accessibility information
                      </Card.Link>
                    </Card.Heading>
                    <ArrowRightCircleIcon />
                  </Card.Content>
                </Card>
              </li>
            ))}
          {permissionsRelevantToCards.includes('users:view') && (
            <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
              <Card cardType="primary" clickable>
                <Card.Content>
                  <Card.Heading className="nhsuk-card__heading nhsuk-heading-m">
                    <Card.Link
                      href={`/manage-your-appointments/site/${site.id}/users`}
                    >
                      Manage users
                    </Card.Link>
                  </Card.Heading>
                  <ArrowRightCircleIcon />
                </Card.Content>
              </Card>
            </li>
          )}
          {permissionsAtAnySite.includes('reports:sitesummary') &&
            siteSummaryEnabled && (
              <li className="nhsuk-grid-column-one-third nhsuk-card-group__item">
                <Card>
                  <Card.Content>
                    <Card.Heading className="nhsuk-card__heading nhsuk-heading-m">
                      <Card.Link href={`/reports`}>Download reports</Card.Link>
                    </Card.Heading>
                    <ArrowRightCircleIcon />
                  </Card.Content>
                </Card>
              </li>
            )}
        </ul>
      )}
    </>
  );
};
