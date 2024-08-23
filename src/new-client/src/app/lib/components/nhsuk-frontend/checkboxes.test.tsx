import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CheckBox, CheckBoxes } from '@nhsuk-frontend-components';

describe('Checkboxes', () => {
  it('renders', () => {
    render(
      <CheckBoxes>
        <CheckBox label={'Apples'} value={'apples'} id="apples"></CheckBox>
        <CheckBox label={'Oranges'} value={'oranges'} id="oranges"></CheckBox>
        <CheckBox label={'Bananas'} value={'bananas'} id="bananas"></CheckBox>
      </CheckBoxes>,
    );

    expect(
      screen.getByRole('checkbox', { name: 'Apples' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('checkbox', { name: 'Oranges' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('checkbox', { name: 'Bananas' }),
    ).toBeInTheDocument();
  });

  it('alters input when clicked', async () => {
    const { user } = render(
      <CheckBoxes>
        <CheckBox label={'Apples'} value={'apples'} id="apples"></CheckBox>
        <CheckBox label={'Oranges'} value={'oranges'} id="oranges"></CheckBox>
        <CheckBox label={'Bananas'} value={'bananas'} id="bananas"></CheckBox>
      </CheckBoxes>,
    );

    const checkBoxOne = screen.getByRole('checkbox', { name: 'Apples' });

    expect(checkBoxOne).not.toBeChecked();
    await user.click(checkBoxOne);
    expect(checkBoxOne).toBeChecked();

    await user.click(checkBoxOne);
    expect(checkBoxOne).not.toBeChecked();
  });
});
