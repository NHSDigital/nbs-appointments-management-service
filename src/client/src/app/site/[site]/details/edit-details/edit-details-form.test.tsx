import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { mockSiteWithAccessibilities } from '@testing/data';
import render from '@testing/render';
import EditDetailsForm from './edit-details-form';
import * as appointmentsService from '@services/appointmentsService';
import { UserEvent } from '@testing-library/user-event';

jest.mock('@services/appointmentsService');

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

const mockSaveSiteDetails = jest.spyOn(appointmentsService, 'saveSiteDetails');

let user: UserEvent;

describe('Edit Site Details Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });

    mockSiteWithAccessibilities.address =
      'A new house, on a new road, in a new city';

    mockSiteWithAccessibilities.phoneNumber = '0118 999 88199 9119 725 3';

    const renderResult = render(
      <EditDetailsForm SiteWithAccessibilities={mockSiteWithAccessibilities} />,
    );

    user = renderResult.user;
  });

  it('renders', async () => {
    expect(screen.getByRole('textbox', { name: 'Site name' })).toBeVisible();
  });

  it('prepopulates the site data correctly in the form', () => {
    expect(screen.getByRole('textbox', { name: 'Site name' })).toHaveValue(
      mockSiteWithAccessibilities.name,
    );

    //TODO textarea label question??
    expect(screen.getByLabelText('Site address')).toHaveValue(
      'A new house,\non a new road,\nin a new city',
    );

    expect(screen.getByRole('textbox', { name: 'Latitude' })).toHaveValue(
      mockSiteWithAccessibilities.location.coordinates[0].toString(),
    );

    expect(screen.getByRole('textbox', { name: 'Longitude' })).toHaveValue(
      mockSiteWithAccessibilities.location.coordinates[1].toString(),
    );

    expect(screen.getByRole('textbox', { name: 'Phone number' })).toHaveValue(
      '0118 999 88199 9119 725 3',
    );
  });

  it('submits and removes the line breaks from the address field', async () => {
    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    //address resaved just as a single comma delimited string
    const expectedPayload = {
      name: mockSiteWithAccessibilities.name,
      address: mockSiteWithAccessibilities.address,
      phoneNumber: '0118 999 88199 9119 725 3',
      latitude: mockSiteWithAccessibilities.location.coordinates[0].toString(),
      longitude: mockSiteWithAccessibilities.location.coordinates[1].toString(),
    };

    expect(mockSaveSiteDetails).toHaveBeenCalledWith(
      mockSiteWithAccessibilities.id,
      expectedPayload,
    );
  });

  it('adding a new line break to the address saves as a comma delimited string', async () => {
    //write text to the address field
    const addressInput = screen.getByLabelText('Site address');

    await user.type(addressInput, ',\nUK');

    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    //address resaved just as a single comma delimited string
    //phone number resaved as a numeric string only
    const expectedPayload = {
      name: mockSiteWithAccessibilities.name,
      address: 'A new house, on a new road, in a new city, UK',
      phoneNumber: '0118 999 88199 9119 725 3',
      latitude: mockSiteWithAccessibilities.location.coordinates[0].toString(),
      longitude: mockSiteWithAccessibilities.location.coordinates[1].toString(),
    };

    expect(mockSaveSiteDetails).toHaveBeenCalledWith(
      mockSiteWithAccessibilities.id,
      expectedPayload,
    );
  });
});
