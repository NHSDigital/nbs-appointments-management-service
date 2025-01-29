import { screen } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import {
  mockSites,
  mockSiteWithAttributes,
  mockWellKnownOdsCodeEntries,
} from '@testing/data';
import render from '@testing/render';
import * as appointmentsService from '@services/appointmentsService';
import EditReferenceDetailsForm from './edit-reference-details-form';

jest.mock('@services/appointmentsService');

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

const mockSaveSiteReferenceDetails = jest.spyOn(
  appointmentsService,
  'saveSiteReferenceDetails',
);

describe('Edit Site Reference Details Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('renders', async () => {
    render(
      <EditReferenceDetailsForm
        siteWithAttributes={mockSiteWithAttributes}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(screen.getByRole('textbox', { name: 'ODS code' })).toBeVisible();
  });

  it('prepopulates the site reference data correctly in the form - well defined', () => {
    render(
      <EditReferenceDetailsForm
        siteWithAttributes={mockSiteWithAttributes}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(screen.getByRole('textbox', { name: 'ODS code' })).toHaveValue(
      mockSiteWithAttributes.odsCode,
    );

    expect(screen.getByRole('combobox', { name: 'ICB' })).toHaveValue(
      mockSiteWithAttributes.integratedCareBoard,
    );

    expect(screen.getByRole('combobox', { name: 'Region' })).toHaveValue(
      mockSiteWithAttributes.region,
    );

    const option1 = screen.getByRole('option', {
      name: 'Integrated Care Board One',
    }) as HTMLOptionElement;
    const option2 = screen.getByRole('option', {
      name: 'Integrated Care Board Three',
    }) as HTMLOptionElement;
    const option3 = screen.getByRole('option', {
      name: 'Region One',
    }) as HTMLOptionElement;
    const option4 = screen.getByRole('option', {
      name: 'Region Three',
    }) as HTMLOptionElement;

    expect(option1.selected).toBe(true);
    expect(option2.selected).toBe(false);
    expect(option3.selected).toBe(true);
    expect(option4.selected).toBe(false);
  });

  it('prepopulates the site reference data correctly in the form - not well defined', () => {
    const site = {
      id: mockSites[3].id,
      address: mockSites[3].address,
      phoneNumber: mockSites[3].phoneNumber,
      name: mockSites[3].name,
      odsCode: mockSites[3].odsCode,
      integratedCareBoard: mockSites[3].integratedCareBoard,
      region: mockSites[3].region,
      location: mockSites[3].location,
      attributeValues: [],
    };

    render(
      <EditReferenceDetailsForm
        siteWithAttributes={site}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    expect(screen.getByRole('textbox', { name: 'ODS code' })).toHaveValue(
      site.odsCode,
    );

    const icbSelect = screen.getByRole('combobox', {
      name: 'ICB',
    }) as HTMLSelectElement;

    expect(icbSelect.value).toBe('');

    const regionSelect = screen.getByRole('combobox', {
      name: 'Region',
    }) as HTMLSelectElement;

    expect(regionSelect.value).toBe('');

    const option1 = screen.getByRole('option', {
      name: 'Integrated Care Board One',
    }) as HTMLOptionElement;
    const option2 = screen.getByRole('option', {
      name: 'Integrated Care Board Three',
    }) as HTMLOptionElement;
    const option3 = screen.getByRole('option', {
      name: 'Region One',
    }) as HTMLOptionElement;
    const option4 = screen.getByRole('option', {
      name: 'Region Three',
    }) as HTMLOptionElement;

    expect(option1.selected).toBe(false);
    expect(option2.selected).toBe(false);
    expect(option3.selected).toBe(false);
    expect(option4.selected).toBe(false);
  });

  it('submitting data uses the ODS code in the option value on save payload', async () => {
    const { user } = render(
      <EditReferenceDetailsForm
        siteWithAttributes={mockSiteWithAttributes}
        wellKnownOdsCodeEntries={mockWellKnownOdsCodeEntries}
      />,
    );

    const icbSelect = screen.getByRole('combobox', { name: 'ICB' });

    await user.selectOptions(icbSelect, 'Integrated Care Board Three');
    expect(icbSelect).toHaveValue('ICB3');

    const regionSelect = screen.getByRole('combobox', { name: 'Region' });

    await user.selectOptions(regionSelect, 'Region Three');
    expect(regionSelect).toHaveValue('R3');

    const saveButton = screen.getByRole('button', {
      name: 'Save and continue',
    });
    await user.click(saveButton);

    const expectedPayload = {
      odsCode: mockSiteWithAttributes.odsCode,
      icb: 'ICB3',
      region: 'R3',
    };

    expect(mockSaveSiteReferenceDetails).toHaveBeenCalledWith(
      mockSiteWithAttributes.id,
      expectedPayload,
    );
  });
});
