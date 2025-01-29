import { Card, SummaryList } from '@components/nhsuk-frontend';
import {
  fetchAttributeDefinitions,
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
  const attributeDefinitions = await fetchAttributeDefinitions();
  const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
    ad.id.startsWith('accessibility'),
  );
  const site = await fetchSite(siteId);
  const informationForCitizenAttribute = site.attributeValues.find(
    sa => sa.id === 'site_details/info_for_citizen',
  );

  const siteReferenceSummaryData = mapSiteReferenceSummaryData(
    site,
    wellKnownOdsEntries,
  );
  const siteCoreSummary = mapCoreSiteSummaryData(site);

  return (
    <>
      <Card title="Site reference details">
        {siteReferenceSummaryData && (
          <SummaryList {...siteReferenceSummaryData}></SummaryList>
        )}
      </Card>
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
      <Card title="Access needs">
        <SummaryList
          borders={true}
          items={accessibilityAttributeDefinitions.map(definition => {
            return {
              title: definition.displayName,
              value:
                site?.attributeValues.find(value => value.id === definition.id)
                  ?.value === 'true'
                  ? 'Yes'
                  : 'No',
            };
          })}
        />
        {permissions.includes('site:manage') ? (
          <Link
            href={`/site/${site.id}/details/edit-attributes`}
            className="nhsuk-link"
          >
            Edit access needs
          </Link>
        ) : null}
      </Card>
      <Card title="Information for citizens">
        {informationForCitizenAttribute ? (
          <p>{informationForCitizenAttribute.value}</p>
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
