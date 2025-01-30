import { render, screen } from '@testing-library/react';
import { EditAccessibilitiesPage } from './edit-accessibilities-page';
import { AccessibilityDefinition, AccessibilityValue, FullSite } from '@types';
import {
  fetchAccessibilityDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import { mockAccessibilityDefinitions, mockFullSite } from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchAccessibilityDefinitionsMock =
  fetchAccessibilityDefinitions as jest.Mock<
    Promise<AccessibilityDefinition[]>
  >;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<FullSite>>;

jest.mock('./add-accessibilities-form', () => {
  const MockForm = ({
    accessibilityDefinitions,
    site,
    accessibilityValues,
  }: {
    accessibilityDefinitions: AccessibilityDefinition[];
    site: string;
    accessibilityValues: AccessibilityValue[];
  }) => {
    return (
      <>
        <div>Add Accessibility Form</div>
        {accessibilityValues.map(av => (
          <div key={av.id}>
            id={av.id} value={av.value}
          </div>
        ))}
        {accessibilityDefinitions.map(ad => (
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

describe('Manage Accessibilities Page', () => {
  beforeEach(() => {
    fetchAccessibilityDefinitionsMock.mockResolvedValue(
      mockAccessibilityDefinitions,
    );
    fetchSiteMock.mockResolvedValue(mockFullSite);
  });

  it('renders', async () => {
    const jsx = await EditAccessibilitiesPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(
      screen.getByText('Configure your current site details'),
    ).toBeVisible();
  });

  it('calls fetch accessibility values with correct site id', async () => {
    const jsx = await EditAccessibilitiesPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(fetchSite).toHaveBeenCalledWith('TEST');
  });

  it('passes props to form component', async () => {
    const jsx = await EditAccessibilitiesPage({
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
