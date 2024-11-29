import render from '@testing/render';
import { screen } from '@testing-library/react';
import { Fieldset } from '@nhsuk-frontend-components';

describe('Fieldset', () => {
  it('renders', () => {
    render(
      <Fieldset legend="What is your address?">
        <div>Some inner content</div>
      </Fieldset>,
    );

    expect(
      screen.getByRole('heading', { name: 'What is your address?' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Some inner content')).toBeInTheDocument();
  });
});
