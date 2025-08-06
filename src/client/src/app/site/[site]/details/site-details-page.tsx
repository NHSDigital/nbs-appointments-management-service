import { Card, SummaryList } from '@components/nhsuk-frontend';
import {
  fetchAccessibilityDefinitions,
  fetchFeatureFlag,
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
  const [accessibilityDefinitions, site, siteStatus] = await Promise.all([
    fetchAccessibilityDefinitions(),
    fetchSite(siteId),
    fetchFeatureFlag('SiteStatus'),
  ]);

  const siteReferenceSummaryData = mapSiteReferenceSummaryData(
    site,
    wellKnownOdsEntries,
  );
  const siteCoreSummary = mapCoreSiteSummaryData(site, siteStatus.enabled);

  return (
    <>
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
      <Card title="Site details">
        {siteCoreSummary && <SummaryList {...siteCoreSummary} />}
        {permissions.includes('site:manage') ? (
          <>
            <Link
              href={`/site/${site.id}/details/edit-details`}
              className="nhsuk-link"
            >
              Edit site details
            </Link>
            |
            <Link
              href={`/site/${site.id}/details/edit-site-status`}
              className="nhsuk-link"
            >
              Edit Site Status
            </Link>
          </>
        ) : null}
      </Card>
      <Card title="Access needs">
        <SummaryList
          borders={true}
          items={accessibilityDefinitions.map(definition => {
            return {
              title: definition.displayName,
              value:
                site?.accessibilities.find(value => value.id === definition.id)
                  ?.value === 'true'
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
    </>
  );
};

export default SiteDetailsPage;
