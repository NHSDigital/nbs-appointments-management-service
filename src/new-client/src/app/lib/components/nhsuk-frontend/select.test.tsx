import render from '@testing/render';
import { screen } from '@testing-library/react';
import { Select } from '@nhsuk-frontend-components';

const mockOptions = [
  { value: 'apples', label: 'Apples' },
  { value: 'oranges', label: 'Oranges' },
  { value: 'bananas', label: 'Bananas' },
];

describe('Select', () => {
  it('renders', () => {
    render(<Select options={mockOptions} />);

    expect(screen.getByRole('combobox')).toBeInTheDocument();
  });

  it('allows the user to select an option', async () => {
    const { user } = render(<Select options={mockOptions} />);

    const selectElement = screen.getByRole('combobox');
    expect(selectElement).toHaveValue('apples');

    await user.selectOptions(selectElement, 'oranges');
    expect(selectElement).toHaveValue('oranges');
  });
});
