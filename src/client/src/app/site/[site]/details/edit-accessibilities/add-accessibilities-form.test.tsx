import { screen } from '@testing-library/react';
import {
  mockAccessibilityDefinitions,
  mockAccessibilities,
} from '@testing/data';
import AddAccessibilitiesForm from './add-accessibilities-form';
import { useRouter } from 'next/navigation';
import render from '@testing/render';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockSaveSiteAccessibilities = jest.spyOn(
  appointmentsService,
  'saveSiteAccessibilities',
);

describe('Add Accessibilities Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });
  it('displays a check box for each available role', () => {
    render(
      <AddAccessibilitiesForm
        accessibilityDefinitions={mockAccessibilityDefinitions}
        site="TEST"
        accessibilities={mockAccessibilities}
      />,
    );

    expect(
      screen.getByRole('checkbox', { name: 'Accessibility attribute 1' }),
    ).toBeVisible();
    expect(
      screen.getByRole('checkbox', { name: 'Accessibility attribute 2' }),
    ).toBeVisible();
  });

  it('checks the correct options', () => {
    render(
      <AddAccessibilitiesForm
        accessibilityDefinitions={mockAccessibilityDefinitions}
        site="TEST"
        accessibilities={mockAccessibilities}
      />,
    );

    expect(
      screen.getByRole('checkbox', { name: 'Accessibility attribute 1' }),
    ).toBeChecked();
    expect(
      screen.getByRole('checkbox', { name: 'Accessibility attribute 2' }),
    ).not.toBeChecked();
  });

  it('returns the user to the site page when they cancel', async () => {
    const { user } = render(
      <AddAccessibilitiesForm
        accessibilityDefinitions={mockAccessibilityDefinitions}
        site="TEST"
        accessibilities={mockAccessibilities}
      />,
    );
    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/details');
  });

  it('calls the save function when saved', async () => {
    const { user } = render(
      <AddAccessibilitiesForm
        accessibilityDefinitions={mockAccessibilityDefinitions}
        site="TEST"
        accessibilities={mockAccessibilities}
      />,
    );
    const checkBox = screen.getByRole('checkbox', {
      name: 'Accessibility attribute 2',
    });
    await user.click(checkBox);
    const saveButton = screen.getByRole('button', {
      name: 'Confirm site details',
    });
    await user.click(saveButton);

    const expectedPayload = {
      accessibilities: [
        { id: 'accessibility/attr_1', value: 'true' },
        { id: 'accessibility/attr_2', value: 'true' },
        { id: 'different_accessibility_set/attr_1', value: 'false' },
      ],
    };

    expect(mockSaveSiteAccessibilities).toHaveBeenCalledWith(
      'TEST',
      expectedPayload,
    );
  });
});
