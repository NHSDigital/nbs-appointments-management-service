import { screen } from '@testing-library/react';
import AddInformationForCitizensForm from './add-information-for-citizens-form';
import render from '@testing/render';
import { useRouter } from 'next/navigation';
import * as appointmentsService from '@services/appointmentsService';

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockReplace = jest.fn();

jest.mock('@services/appointmentsService');
const mockSetSiteInformationForCitizen = jest.spyOn(
  appointmentsService,
  'setSiteInformationForCitizen',
);

describe('Add Information For Citizen Form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      replace: mockReplace,
    });
  });

  it('displays a text area to input the information for citizens', () => {
    render(<AddInformationForCitizensForm information="" site="TEST" />);

    expect(
      screen.getByRole('textbox', {
        name: /What information would you like to include?/i,
      }),
    ).toBeInTheDocument();
  });

  it('returns the user to the site page when they cancel', async () => {
    const { user } = render(
      <AddInformationForCitizensForm information="" site="TEST" />,
    );
    const cancelButton = screen.getByRole('button', { name: 'Cancel' });
    await user.click(cancelButton);

    expect(mockReplace).toHaveBeenCalledWith('/site/TEST/details');
  });

  it('calls the save function when saved', async () => {
    const { user } = render(
      <AddInformationForCitizensForm information="" site="TEST" />,
    );
    const userInput = 'test user input';
    const textArea = screen.getByRole('textbox', {
      name: /What information would you like to include?/i,
    });
    await user.type(textArea, userInput);
    const saveButton = screen.getByRole('button', {
      name: 'Confirm site details',
    });
    await user.click(saveButton);

    const expectedPayload = {
      informationForCitizens: userInput,
    };

    expect(mockSetSiteInformationForCitizen).toHaveBeenCalledWith(
      'TEST',
      expectedPayload,
    );
  });

  it.each([
    'test user input which is inv@lid! with special characters.',
    'test user input which is invalid with a URL www.test.com.',
    'test user input which in invalid because of an _ underscore.',
  ])(
    'should display a validation error when user input is invalid',
    async (textInput: string) => {
      const { user } = render(
        <AddInformationForCitizensForm information="" site="TEST" />,
      );
      const textArea = screen.getByRole('textbox', {
        name: /What information would you like to include?/i,
      });
      await user.type(textArea, textInput);
      const saveButton = screen.getByRole('button', {
        name: 'Confirm site details',
      });
      await user.click(saveButton);

      expect(
        screen.getByText(
          'Site information cannot contain a URL or special characters except full stops, commas, and hyphens',
        ),
      ).toBeInTheDocument();
      expect(mockSetSiteInformationForCitizen).not.toHaveBeenCalled();
    },
  );

  it('should display characters remaining text', async () => {
    const { user } = render(
      <AddInformationForCitizensForm information="" site="TEST" />,
    );
    const textArea = screen.getByRole('textbox', {
      name: /What information would you like to include?/i,
    });
    await user.type(textArea, 'test user input');

    expect(
      screen.getByText('You have 135 characters remaining'),
    ).toBeInTheDocument();
  });

  it('should allow an empty input', async () => {
    const { user } = render(
      <AddInformationForCitizensForm information="" site="TEST" />,
    );

    const saveButton = screen.getByRole('button', {
      name: 'Confirm site details',
    });
    await user.click(saveButton);

    const expectedPayload = {
      informationForCitizens: '',
    };

    expect(mockSetSiteInformationForCitizen).toHaveBeenCalledWith(
      'TEST',
      expectedPayload,
    );
  });
});
