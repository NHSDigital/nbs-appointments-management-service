import { render, screen } from '@testing-library/react';
import { ManageAttributesPage } from './manage-attributes-page';
import { AttributeDefinition, AttributeValue } from '@types';
import {
  fetchAttributeDefinitions,
  fetchSiteAttributeValues,
} from '@services/appointmentsService';
import { mockAttributeDefinitions, mockAttributeValues } from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchAttributeDefinitionsMock = fetchAttributeDefinitions as jest.Mock<
  Promise<AttributeDefinition[]>
>;

jest.mock('@services/appointmentsService');
const fetchSiteAttributeValuesMock = fetchSiteAttributeValues as jest.Mock<
  Promise<AttributeValue[]>
>;

jest.mock('./add-attributes-form', () => {
  const MockForm = ({
    attributeDefinitions,
    site,
    attributeValues,
  }: {
    attributeDefinitions: AttributeDefinition[];
    site: string;
    attributeValues: AttributeValue[];
  }) => {
    return (
      <>
        <div>Add Attribute Form</div>
        {attributeValues.map(av => (
          <div key={av.id}>
            id={av.id} value={av.value}
          </div>
        ))}
        {attributeDefinitions.map(ad => (
          <div key={ad.id}>
            id={ad.id} displayName={ad.displayName}
          </div>
        ))}
        <div>site={site}</div>
      </>
    );
  };
  return MockForm;
});

describe('Manage Attributes Page', () => {
  beforeEach(() => {
    fetchAttributeDefinitionsMock.mockResolvedValue(mockAttributeDefinitions);
    fetchSiteAttributeValuesMock.mockResolvedValue(mockAttributeValues);
  });

  it('renders', async () => {
    const jsx = await ManageAttributesPage({ site: 'TEST' });
    render(jsx);
    expect(
      screen.getByText('Configure your current site details'),
    ).toBeVisible();
  });

  it('calls fetch attribute values with correct site id', async () => {
    const jsx = await ManageAttributesPage({ site: 'TEST' });
    render(jsx);
    expect(fetchSiteAttributeValues).toHaveBeenCalledWith('TEST');
  });

  it('passes props to form component', async () => {
    const jsx = await ManageAttributesPage({ site: 'TEST' });
    render(jsx);

    expect(
      screen.getByText('id=accessibility/attr_1 value=true'),
    ).toBeVisible();
    expect(
      screen.getByText(
        'id=accessibility/attr_1 displayName=Accessibility attribute 1',
      ),
    ).toBeVisible();
    expect(
      screen.getByText(
        'id=accessibility/attr_2 displayName=Accessibility attribute 2',
      ),
    ).toBeVisible();
    expect(
      screen.queryByText(
        'id=different_attribute_set/attr_1 displayName=Different attribute set attribute 1',
      ),
    ).not.toBeInTheDocument();
  });
});
