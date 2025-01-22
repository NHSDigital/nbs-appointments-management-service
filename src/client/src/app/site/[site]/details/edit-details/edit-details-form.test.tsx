import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { mockSiteWithAttributes } from '@testing/data';
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

    mockSiteWithAttributes.address =
      'A new house, on a new road, in a new city';

    mockSiteWithAttributes.phoneNumber = '0118 999 88199 9119 725 3';

    const renderResult = render(
      <EditDetailsForm siteWithAttributes={mockSiteWithAttributes} />,
    );

    user = renderResult.user;
  });

  it('renders', async () => {
    expect(
      screen.getByRole('heading', { level: 3, name: 'Site name' }),
    ).toBeVisible();
  });

  it('prepopulates the site data correctly in the form', () => {
    const textInputs = screen.getAllByRole('textbox');

    expect(textInputs).toHaveLength(5);
    expect(textInputs[0]).toHaveValue(mockSiteWithAttributes.name);

    //formats the address in the textarea with line breaks
    expect(textInputs[1]).toHaveValue(
      'A new house,\non a new road,\nin a new city',
    );
    expect(textInputs[2]).toHaveValue(
      mockSiteWithAttributes.location.coordinates[0].toString(),
    );
    expect(textInputs[3]).toHaveValue(
      mockSiteWithAttributes.location.coordinates[1].toString(),
    );
    //removes whitespace as only allows numbers
    expect(textInputs[4]).toHaveValue('01189998819991197253');
  });

  it('submits and removes the line breaks from the address field', async () => {
    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    //address resaved just as a single comma delimited string
    //phone number resaved as a numeric string only
    const expectedPayload = {
      name: mockSiteWithAttributes.name,
      address: mockSiteWithAttributes.address,
      phoneNumber: '01189998819991197253',
      latitude: mockSiteWithAttributes.location.coordinates[0].toString(),
      longitude: mockSiteWithAttributes.location.coordinates[1].toString(),
    };

    expect(mockSaveSiteDetails).toHaveBeenCalledWith(
      mockSiteWithAttributes.id,
      expectedPayload,
    );
  });

  it('adding a new line break to the address saves as a comma delimited string', async () => {
    //write text to the address field
    const allInputs = screen.getAllByRole('textbox');
    const addressInput = allInputs[1];

    await user.type(addressInput, ',\nUK');

    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    //address resaved just as a single comma delimited string
    //phone number resaved as a numeric string only
    const expectedPayload = {
      name: mockSiteWithAttributes.name,
      address: 'A new house, on a new road, in a new city, UK',
      phoneNumber: '01189998819991197253',
      latitude: mockSiteWithAttributes.location.coordinates[0].toString(),
      longitude: mockSiteWithAttributes.location.coordinates[1].toString(),
    };

    expect(mockSaveSiteDetails).toHaveBeenCalledWith(
      mockSiteWithAttributes.id,
      expectedPayload,
    );
  });
});
