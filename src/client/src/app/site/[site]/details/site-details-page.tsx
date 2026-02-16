import fromServer from '@server/fromServer';
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
import { Card, SummaryList } from 'nhsuk-react-components';

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
    fromServer(fetchAccessibilityDefinitions()),
    fromServer(fetchSite(siteId)),
    fromServer(fetchFeatureFlag('SiteStatus')),
  ]);

  const siteReferenceSummaryData = mapSiteReferenceSummaryData(
    site,
    wellKnownOdsEntries,
  );
  const siteCoreSummary = mapCoreSiteSummaryData(site, siteStatus.enabled);

  return (
    <ol className="card-list">
      <li>
        <Card>
          <Card.Heading>{site.name}</Card.Heading>
          {permissions.includes('site:manage') ? (
            <Card.Action
              href={`/manage-your-appointments/site/${site.id}/details/edit-details`}
            >
              Edit site details
            </Card.Action>
          ) : null}
          {siteStatus.enabled && permissions.includes('site:manage') ? (
            <Card.Action
              href={`/manage-your-appointments/site/${site.id}/details/edit-site-status`}
            >
              Change site status
            </Card.Action>
          ) : null}
          <SummaryList>
            {siteCoreSummary?.items.map((item, index) => (
              <SummaryList.Row key={index}>
                <SummaryList.Key>{item.title}</SummaryList.Key>
                <SummaryList.Value>
                  {typeof item.value === 'string' ? (
                    item.tag !== undefined ? (
                      <span
                        className={`nhsuk-tag nhsuk-tag--${item.tag?.colour}`}
                      >
                        {item.value}
                      </span>
                    ) : (
                      item.value
                    )
                  ) : (
                    <div>
                      {item.value?.map((line, lineIndex) => (
                        <div key={lineIndex}>{line}</div>
                      ))}
                    </div>
                  )}
                </SummaryList.Value>
              </SummaryList.Row>
            ))}
          </SummaryList>
        </Card>
        <Card>
          <Card.Heading>Site reference details</Card.Heading>
          {permissions.includes('site:manage:admin') && (
            <Card.Action
              href={`/manage-your-appointments/site/${site.id}/details/edit-reference-details`}
            >
              Edit site reference details
            </Card.Action>
          )}
          <SummaryList>
            {siteReferenceSummaryData?.items.map((item, index) => (
              <SummaryList.Row key={index}>
                <SummaryList.Key>{item.title}</SummaryList.Key>
                <SummaryList.Value>
                  {typeof item.value === 'string' ? (
                    item.value
                  ) : (
                    <div>
                      {item.value?.map((line, lineIndex) => (
                        <div key={lineIndex}>{line}</div>
                      ))}
                    </div>
                  )}
                </SummaryList.Value>
              </SummaryList.Row>
            ))}
          </SummaryList>
        </Card>
        <Card>
          <Card.Heading>Site accessibility information</Card.Heading>
          {permissions.includes('site:manage') && (
            <Card.Action
              href={`/manage-your-appointments/site/${site.id}/details/edit-accessibilities`}
            >
              Edit site accessibility information
            </Card.Action>
          )}
          <SummaryList>
            {accessibilityDefinitions.map((definition, index) => {
              const accessibilityForDefinition = site?.accessibilities.find(
                value => value.id === definition.id,
              );
              return (
                <SummaryList.Row key={index}>
                  <SummaryList.Key>{definition.displayName}</SummaryList.Key>
                  <SummaryList.Value>
                    {accessibilityForDefinition?.value.toLowerCase() === 'true'
                      ? 'Yes'
                      : 'No'}
                  </SummaryList.Value>
                </SummaryList.Row>
              );
            })}
          </SummaryList>
        </Card>
        <Card>
          <Card.Heading>Information for citizens</Card.Heading>
          {permissions.includes('site:manage') && (
            <Card.Action
              href={`/manage-your-appointments/site/${site.id}/details/edit-information-for-citizens`}
            >
              Edit information for citizens
            </Card.Action>
          )}
          {site.informationForCitizens ? (
            <p>{site.informationForCitizens}</p>
          ) : (
            <p>Information for people visiting the site</p>
          )}
        </Card>
      </li>
    </ol>
  );
};

export default SiteDetailsPage;
