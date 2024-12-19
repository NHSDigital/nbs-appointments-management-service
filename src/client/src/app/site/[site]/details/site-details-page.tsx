import { Card, SummaryList } from '@components/nhsuk-frontend';
import {
  fetchAttributeDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import { mapSiteSummaryData } from '@services/siteService';
import { Site } from '@types';
import Link from 'next/link';

type Props = {
  site: Site;
  permissions: string[];
};

const SiteDetailsPage = async ({ site, permissions }: Props) => {
  const attributeDefinitions = await fetchAttributeDefinitions();
  const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
    ad.id.startsWith('accessibility'),
  );
  const siteDetails = await fetchSite(site.id);
  const informationForCitizenAttribute = siteDetails.attributeValues.find(
    sa => sa.id === 'site_details/info_for_citizen',
  );

  const siteSummary = mapSiteSummaryData(site);

  return (
    <>
      <Card title="Site Details">
        {siteSummary && <SummaryList {...siteSummary}></SummaryList>}
        {/* TODO: Add link to edit site details */}
      </Card>
      <Card title="Access needs">
        <SummaryList
          borders={true}
          items={accessibilityAttributeDefinitions.map(definition => {
            return {
              title: definition.displayName,
              value:
                siteDetails?.attributeValues.find(
                  value => value.id === definition.id,
                )?.value === 'true'
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
