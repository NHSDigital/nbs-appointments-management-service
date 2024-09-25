import { Button, ButtonGroup, SummaryList } from '@components/nhsuk-frontend';
import AddAttributesForm from './add-attributes-form';
import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';

type Props = {
  site: string;
  permissions: string[];
};

export const SiteAttributesPage = async ({ site, permissions }: Props) => {
  const attributeDefinitions = await fetchAttributeDefinitions();
  const accessibilityAttributeDefinitions = attributeDefinitions.filter(ad =>
    ad.id.startsWith('accessibility'),
  );
  const siteAttributeValues = await fetchSiteAttributeValues(site);

  return permissions.includes('site:manage') ? (
    <>
      <div className="nhsuk-form-group">
        <div className="nhsuk-hint">Configure your current site details</div>
      </div>
      <AddAttributesForm
        attributeDefinitions={accessibilityAttributeDefinitions}
        site={site}
        attributeValues={siteAttributeValues}
      />
    </>
  ) : (
    <>
      <h3>Location</h3>
      <p>
        This is a paragraph of text This is a paragraph of text This is a
        paragraph of text This is a paragraph of text This is a paragraph of
        text This is a paragraph of text This is a paragraph of text This is a
        paragraph of text This is a paragraph of text
      </p>

      <ButtonGroup>
        <Button href={`/site/${site}`}>Manage location</Button>
        <Button href={`/site/${site}`} styleType="secondary">
          Go back
        </Button>
      </ButtonGroup>

      <h3>Section 2</h3>
      <p>
        This is a paragraph of text This is a paragraph of text This is a
        paragraph of text This is a paragraph of text This is a paragraph of
        text This is a paragraph of text This is a paragraph of text This is a
        paragraph of text This is a paragraph of text
      </p>
      <ButtonGroup>
        <Button href={`/site/${site}`}>Manage section 2</Button>
        <Button href={`/site/${site}`} styleType="secondary">
          Go back
        </Button>
      </ButtonGroup>
      <h3>Access needs</h3>
      <p>The access needs your current site offers</p>

      <SummaryList
        items={accessibilityAttributeDefinitions.map(value => {
          return {
            title: value.displayName,
            value: siteAttributeValues
              .map(siteAttributeValue => siteAttributeValue.id)
              .includes(value.id)
              ? 'Status: Active'
              : 'Status: Inactive',
            action: {
              text: 'Change',
              href: `${site}/attributes/${value.id}`,
            },
          };
        })}
      />
      <ButtonGroup>
        <Button href={`/site/${site}`}>Manage site details</Button>
        <Button href={`/site/${site}`} styleType="secondary">
          Go back
        </Button>
      </ButtonGroup>
    </>
  );
};
