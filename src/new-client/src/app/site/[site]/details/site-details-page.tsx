import { Button, ButtonGroup, SummaryList } from '@components/nhsuk-frontend';
import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';
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
  const siteAttributeValues = await fetchSiteAttributeValues(site.id);
  const informationForCitizenAttribute = siteAttributeValues.filter(sa =>
    sa.id.includes('info_for_citizen'),
  )[0];

  return (
    <>
      <h3>Access needs</h3>
      <p>The access needs your current site offers</p>

      <SummaryList
        borders={false}
        items={accessibilityAttributeDefinitions.map(definition => {
          return {
            title: definition.displayName,
            value:
              siteAttributeValues.find(value => value.id === definition.id)
                ?.value === 'true'
                ? 'Status: Active'
                : 'Status: Inactive',
          };
        })}
      />
      {permissions.includes('site:manage') ? (
        <ButtonGroup>
          <Link href={`/site/${site.id}/details/edit-attributes`}>
            <Button>Manage site details</Button>
          </Link>
          <Link href={`/site/${site.id}`}>
            <Button type="button" styleType="secondary">
              Go back
            </Button>
          </Link>
        </ButtonGroup>
      ) : (
        <Link href={`/site/${site.id}`}>
          <Button type="button" styleType="secondary">
            Go back
          </Button>
        </Link>
      )}

      <h3>Information for citizens</h3>
      <p>Instructions to be show to people when they arrive</p>
      {informationForCitizenAttribute.value ? (
        <p>{informationForCitizenAttribute.value}</p>
      ) : (
        ''
      )}
      {permissions.includes('site:manage') ? (
        <ButtonGroup>
          <Link href={`/site/${site.id}/details/edit-information-for-citizens`}>
            <Button>Manage information for citizens</Button>
          </Link>
          <Link href={`/site/${site.id}`}>
            <Button type="button" styleType="secondary">
              Go back
            </Button>
          </Link>
        </ButtonGroup>
      ) : (
        <Link href={`/site/${site.id}`}>
          <Button type="button" styleType="secondary">
            Go back
          </Button>
        </Link>
      )}
    </>
  );
};

export default SiteDetailsPage;
