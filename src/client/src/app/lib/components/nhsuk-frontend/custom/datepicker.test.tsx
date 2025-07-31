import render from '@testing/render';
import { screen } from '@testing-library/react';
import Datepicker from './datepicker';

describe('Datepicker', () => {
  it('renders', () => {
    render(
      <Datepicker
        id="some-test-id"
        label="Some test label"
        hint="For example, 17/05/2024."
        min={'17/05/2024'}
        max={'29/05/2024'}
      />,
    );

    expect(screen.getByLabelText('Some test label')).toBeInTheDocument();
  });

  it('displays a hint', () => {
    render(
      <Datepicker
        id="some-test-id"
        label="Some test label"
        hint="For example, 17/05/2024."
      />,
    );

    const hintSpan = screen.getByText('For example, 17/05/2024.');
    const input = screen.getByLabelText('Some test label');

    expect(input).toHaveAttribute('aria-describedby', hintSpan.id);
  });

  it('permits input', async () => {
    const { user } = render(
      <Datepicker
        id="some-test-id"
        label="Some test label"
        hint="For example, 17/05/2024."
        defaultValue={'2020-01-01'}
      />,
    );

    const input = screen.getByLabelText('Some test label');

    expect(input).toHaveValue('2020-01-01');

    await user.clear(input);
    await user.type(input, '2012-07-18');

    expect(input).toHaveValue('2012-07-18');
  });
});
