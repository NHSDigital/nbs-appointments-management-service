import { render, screen } from '@testing-library/react';
import {
  fetchAccessibilityDefinitions,
  fetchSite,
} from '@services/appointmentsService';
import { mockAccessibilityDefinitions, mockSite } from '@testing/data';
import { Accessibility, AccessibilityDefinition, Site } from '@types';
import EditAccessibilitiesPage from './edit-accessibilities-page';

jest.mock('@services/appointmentsService');
const fetchAccessibilityDefinitionsMock =
  fetchAccessibilityDefinitions as jest.Mock<
    Promise<AccessibilityDefinition[]>
  >;

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<Site>>;

jest.mock('./add-accessibilities-form', () => {
  const MockForm = ({
    accessibilityDefinitions,
    site,
    accessibilities,
  }: {
    accessibilityDefinitions: AccessibilityDefinition[];
    site: string;
    accessibilities: Accessibility[];
  }) => {
    return (
      <>
        <div>Add Accessibility Form</div>
        {accessibilities.map(av => (
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
    fetchSiteMock.mockResolvedValue(mockSite);
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
