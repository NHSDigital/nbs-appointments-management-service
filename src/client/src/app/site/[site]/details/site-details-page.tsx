import { Card, SummaryList } from '@components/nhsuk-frontend';
import {
  fetchAccessibilityDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import {
  mapCoreSiteSummaryData,
  mapSiteReferenceSummaryData,
} from '@services/siteService';
import { WellKnownOdsEntry } from '@types';
import Link from 'next/link';

type Props = {
  siteId: string;
  permissions: string[];
  wellKnownOdsEntries: WellKnownOdsEntry[];
};

const SiteDetailsPage = async ({
  siteId,
  permissions,
  wellKnownOdsEntries,
}: Props) => {
  const [accessibilityDefinitions, site] = await Promise.all([
    fetchAccessibilityDefinitions(),
    fetchSite(siteId),
  ]);

  const siteReferenceSummaryData = mapSiteReferenceSummaryData(
    site,
    wellKnownOdsEntries,
  );
  const siteCoreSummary = mapCoreSiteSummaryData(site);

  return (
    <ol className="card-list">
      <li key={`site-reference-summary`}>
        <Card title="Site reference details">
          {siteReferenceSummaryData && (
            <SummaryList {...siteReferenceSummaryData}></SummaryList>
          )}
          {permissions.includes('site:manage:admin') ? (
            <Link
              href={`/site/${site.id}/details/edit-reference-details`}
              className="nhsuk-link"
            >
              Edit site reference details
            </Link>
          ) : null}
        </Card>
      </li>
      <li key={`site-details-summary`}>
        <Card title="Site details">
          {siteCoreSummary && <SummaryList {...siteCoreSummary} />}
          {permissions.includes('site:manage') ? (
            <Link
              href={`/site/${site.id}/details/edit-details`}
              className="nhsuk-link"
            >
              Edit site details
            </Link>
          ) : null}
        </Card>
      </li>
      <li key={`site-access-needs-summary`}>
        <Card title="Access needs">
          <SummaryList
            borders={true}
            items={accessibilityDefinitions.map(definition => {
              return {
                title: definition.displayName,
                value:
                  site?.accessibilities.find(
                    value => value.id === definition.id,
                  )?.value === 'true'
                    ? 'Yes'
                    : 'No',
              };
            })}
          />
          {permissions.includes('site:manage') ? (
            <Link
              href={`/site/${site.id}/details/edit-accessibilities`}
              className="nhsuk-link"
            >
              Edit access needs
            </Link>
          ) : null}
        </Card>
      </li>
      <li key={`site-citizen-information-summary`}>
        <Card title="Information for citizens">
          {site.informationForCitizens ? (
            <p>{site.informationForCitizens}</p>
          ) : (
            <p>Information for people visiting the site</p>
          )}
          {permissions.includes('site:manage') ? (
            <Link
              href={`/site/${site.id}/details/edit-information-for-citizens`}
              className="nhsuk-link"
            >
              Edit information for citizens
            </Link>
          ) : null}
        </Card>
      </li>
    </ol>
  );
};

export default SiteDetailsPage;
