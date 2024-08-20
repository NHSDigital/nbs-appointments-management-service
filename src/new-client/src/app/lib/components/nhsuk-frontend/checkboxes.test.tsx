import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CheckBoxes, CheckBoxProps } from '@nhsuk-frontend-components';

const mockCheckboxes: CheckBoxProps[] = [
  {
    prompt: 'Apples',
    fieldValue: 'apples',
    fieldId: 'checkbox-1',
    fieldName: 'apples',
  },
  {
    prompt: 'Oranges',
    fieldValue: 'oranges',
    fieldId: 'checkbox-2',
    fieldName: 'oranges',
  },
  {
    prompt: 'Bananas',
    fieldValue: 'bananas',
    fieldId: 'checkbox-3',
    fieldName: 'bananas',
  },
];

describe('Checkboxes', () => {
  it('renders', () => {
    render(<CheckBoxes checkboxes={mockCheckboxes} />);

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
    const { user } = render(<CheckBoxes checkboxes={mockCheckboxes} />);

    const checkBoxOne = screen.getByRole('checkbox', { name: 'Apples' });

    expect(checkBoxOne).not.toBeChecked();
    await user.click(checkBoxOne);
    expect(checkBoxOne).toBeChecked();

    await user.click(checkBoxOne);
    expect(checkBoxOne).not.toBeChecked();
  });
});
