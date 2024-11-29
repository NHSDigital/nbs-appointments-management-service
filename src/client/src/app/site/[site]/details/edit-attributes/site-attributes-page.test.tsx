import { render, screen } from '@testing-library/react';
import { EditAttributesPage } from './edit-attributes-page';
import {
  AttributeDefinition,
  AttributeValue,
  SiteWithAttributes,
} from '@types';
import {
  fetchAttributeDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import {
  mockAttributeDefinitions,
  mockSiteWithAttributes,
} from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchAttributeDefinitionsMock = fetchAttributeDefinitions as jest.Mock<
  Promise<AttributeDefinition[]>
>;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<SiteWithAttributes>>;

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

const mockPermissions = ['site:manage', 'site:view'];

describe('Manage Attributes Page', () => {
  beforeEach(() => {
    fetchAttributeDefinitionsMock.mockResolvedValue(mockAttributeDefinitions);
    fetchSiteMock.mockResolvedValue(mockSiteWithAttributes);
  });

  it('renders', async () => {
    const jsx = await EditAttributesPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(
      screen.getByText('Configure your current site details'),
    ).toBeVisible();
  });

  it('calls fetch attribute values with correct site id', async () => {
    const jsx = await EditAttributesPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(fetchSite).toHaveBeenCalledWith('TEST');
  });

  it('passes props to form component', async () => {
    const jsx = await EditAttributesPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
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
