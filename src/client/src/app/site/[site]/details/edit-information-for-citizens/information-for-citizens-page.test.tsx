import {
  fetchInformationForCitizens,
  fetchSite,
} from '@services/appointmentsService';
import { AttributeValue, SiteWithAttributes } from '@types';
import { EditInformationForCitizensPage } from './edit-information-for-citizens-page';
import { render, screen } from '@testing-library/react';
import { mockSiteWithAttributes } from '@testing/data';

jest.mock('@services/appointmentsService');
const fetchSiteMock = fetchSite as jest.Mock<Promise<SiteWithAttributes>>;

jest.mock('@services/appointmentsService');
const fetchInformationForCitizensMock =
  fetchInformationForCitizens as jest.Mock<Promise<AttributeValue[]>>;

jest.mock('./add-information-for-citizens-form', () => {
  const MockForm = ({ information }: { information: string }) => {
    return (
      <>
        <div>Add information for citizen form</div>
        <span>{information}</span>
      </>
    );
  };

  return MockForm;
});

const mockPermissions = ['site:manage', 'site:view'];

describe('Manage Information For Citizen Form', () => {
  beforeEach(() => {
    fetchSiteMock.mockResolvedValue(mockSiteWithAttributes);
    fetchInformationForCitizensMock.mockResolvedValue(
      mockSiteWithAttributes.attributeValues,
    );
  });

  it('renders', async () => {
    const jsx = await EditInformationForCitizensPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(
      screen.getByText(
        'Configure the information you wish to display to citizens about the site',
      ),
    ).toBeVisible();
  });

  it('calls fetch information for citizen with correct site id and scope', async () => {
    const jsx = await EditInformationForCitizensPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);
    expect(fetchInformationForCitizens).toHaveBeenCalledWith(
      'TEST',
      'site_details',
    );
  });

  it('passes props to form component', async () => {
    const jsx = await EditInformationForCitizensPage({
      site: 'TEST',
      permissions: mockPermissions,
    });
    render(jsx);

    expect(screen.getByText('Test information')).toBeVisible();
  });
});