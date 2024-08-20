import { screen } from '@testing-library/react';
import render from '../../../testing/render';
import { TextInput } from '@nhsuk-frontend-components';

describe('Text Input', () => {
  it('renders', () => {
    render(
      <TextInput
        fieldId={'123'}
        fieldName={'test-field'}
        prompt={'Enter some text, please:'}
      />,
    );

    expect(
      screen.getByRole('textbox', { name: 'Enter some text, please:' }),
    ).toBeInTheDocument();
  });

  it('permits user input', async () => {
    const { user } = render(
      <TextInput
        fieldId={'123'}
        fieldName={'test-field'}
        prompt={'Enter some text, please:'}
      />,
    );

    const inputElement = screen.getByRole('textbox', {
      name: /enter some text, please:/i,
    });

    await user.type(inputElement, 'some mock user input');

    expect(inputElement).toHaveValue('some mock user input');
  });
});
