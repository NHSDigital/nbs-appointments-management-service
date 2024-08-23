import { Button, ButtonGroup } from '@nhsuk-frontend-components';
import render from '@testing/render';
import { screen } from '@testing-library/react';

describe('ButtonGroup', () => {
  it('renders', () => {
    render(
      <ButtonGroup>
        <Button>Click Me (one)</Button>
        <Button>Click Me (two)</Button>
      </ButtonGroup>,
    );

    expect(screen.getByRole('list')).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Click Me (one)' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Click Me (two)' }),
    ).toBeInTheDocument();
  });
});
