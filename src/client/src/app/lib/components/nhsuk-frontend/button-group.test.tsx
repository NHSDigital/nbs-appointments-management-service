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

  it('renders horizontally by default', () => {
    render(
      <ButtonGroup>
        <Button>Horizontal 1</Button>
        <Button>Horizontal 2</Button>
      </ButtonGroup>,
    );

    const list = screen.getByRole('list');
    expect(list).toHaveStyle({ flexDirection: 'row' });
  });

  it('renders vertically when vertical is true', () => {
    render(
      <ButtonGroup vertical>
        <Button>Vertical 1</Button>
        <Button>Vertical 2</Button>
      </ButtonGroup>,
    );

    const list = screen.getByRole('list');
    expect(list).toHaveStyle({ flexDirection: 'column' });
  });
});
