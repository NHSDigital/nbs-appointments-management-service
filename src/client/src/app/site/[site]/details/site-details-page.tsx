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
import { ReactNode } from 'react';

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

  const siteDetailsActionLinks: ReactNode = permissions.includes(
    'site:manage',
  ) ? (
    <>
      <Link
        href={`/site/${site.id}/details/edit-details`}
        className="nhsuk-link"
      >
        Edit site details
      </Link>
      {siteStatus.enabled ? (
        <>
          &nbsp;|&nbsp;
          <Link
            href={`/site/${site.id}/details/edit-site-status`}
            className="nhsuk-link"
          >
            Change site status
          </Link>
        </>
      ) : null}
    </>
  ) : null;

  const siteReferenceDetailsLink: ReactNode = permissions.includes(
    'site:manage:admin',
  ) ? (
    <Link
      href={`/site/${site.id}/details/edit-reference-details`}
      className="nhsuk-link"
    >
      Edit site reference details
    </Link>
  ) : null;

  const accessNeedsLink: ReactNode = permissions.includes('site:manage') ? (
    <Link
      href={`/site/${site.id}/details/edit-accessibilities`}
      className="nhsuk-link"
    >
      Edit access needs
    </Link>
  ) : null;

  const informationForCitizenLink: ReactNode = permissions.includes(
    'site:manage',
  ) ? (
    <Link
      href={`/site/${site.id}/details/edit-information-for-citizens`}
      className="nhsuk-link"
    >
      Edit information for citizens
    </Link>
  ) : null;

  return (
    <>
      <Card title="Site details" actionLinks={siteDetailsActionLinks}>
        {siteCoreSummary && <SummaryList {...siteCoreSummary} />}
      </Card>
      <Card
        title="Site reference details"
        actionLinks={siteReferenceDetailsLink}
      >
        {siteReferenceSummaryData && (
          <SummaryList {...siteReferenceSummaryData}></SummaryList>
        )}
      </Card>
      <Card title="Access needs" actionLinks={accessNeedsLink}>
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
      </Card>
      <Card
        title="Information for citizens"
        actionLinks={informationForCitizenLink}
      >
        {site.informationForCitizens ? (
          <p>{site.informationForCitizens}</p>
        ) : (
          <p>Information for people visiting the site</p>
        )}
      </Card>
    </>
  );
};

export default SiteDetailsPage;
