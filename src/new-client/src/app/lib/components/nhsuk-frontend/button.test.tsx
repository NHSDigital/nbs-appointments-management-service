import render from '@testing/render';
import { screen } from '@testing-library/react';
import { Button } from '@nhsuk-frontend-components';

describe('Button', () => {
  it('renders', () => {
    render(<Button>Click me!</Button>);

    expect(
      screen.getByRole('button', { name: 'Click me!' }),
    ).toBeInTheDocument();
  });

  it('accepts an onClick handler', async () => {
    const onClick = jest.fn();
    const { user } = render(<Button onClick={onClick}>Click me!</Button>);

    await user.click(screen.getByRole('button'));

    expect(onClick).toHaveBeenCalled();
  });

  it.each([
    { type: 'primary', expectedClass: 'nhsuk-button' },
    { type: 'secondary', expectedClass: 'nhsuk-button--secondary' },
    { type: 'reverse', expectedClass: 'nhsuk-button--reverse' },
    { type: 'warning', expectedClass: 'nhsuk-button--warning' },
  ])(
    'uses the appropriate class for button type $type',
    ({ type, expectedClass }) => {
      render(
        <Button type={type as 'primary' | 'secondary' | 'reverse' | 'warning'}>
          Click me!
        </Button>,
      );

      expect(screen.getByRole('button')).toHaveClass(expectedClass);
    },
  );
});
