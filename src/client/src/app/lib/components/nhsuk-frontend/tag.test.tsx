import render from '@testing/render';
import { screen } from '@testing-library/react';
import { Tag, TagColor } from '@nhsuk-frontend-components';

describe('Tag', () => {
  it('renders', () => {
    render(<Tag text="Test" color="white" />);

    expect(screen.getByText('Test')).toBeInTheDocument();
  });

  it('uses the strong role', () => {
    render(<Tag text="Test" color="white" />);

    expect(screen.getByRole('strong')).toBeInTheDocument();
  });

  it.each([
    ['white' as TagColor, 'nhsuk-tag--white'],
    ['grey' as TagColor, 'nhsuk-tag--grey'],
    ['green' as TagColor, 'nhsuk-tag--green'],
    ['aqua-green' as TagColor, 'nhsuk-tag--aqua-green'],
    ['blue' as TagColor, 'nhsuk-tag--blue'],
    ['purple' as TagColor, 'nhsuk-tag--purple'],
    ['pink' as TagColor, 'nhsuk-tag--pink'],
    ['red' as TagColor, 'nhsuk-tag--red'],
    ['orange' as TagColor, 'nhsuk-tag--orange'],
    ['yellow' as TagColor, 'nhsuk-tag--yellow'],
  ])(
    'renders with the correct styling class',
    (color: TagColor, expectedClass: string) => {
      render(<Tag text="Test" color={color} />);

      expect(screen.getByText('Test')).toHaveClass(expectedClass);
    },
  );

  it('renders without a color prop', () => {
    render(<Tag text="Default" />);

    const tagElement = screen.getByText('Default');
    expect(tagElement).toBeInTheDocument();

    expect(tagElement).toHaveClass('nhsuk-tag');
    expect(tagElement).not.toHaveClass('nhsuk-tag--white');
    expect(tagElement).not.toHaveClass('nhsuk-tag--grey');
    expect(tagElement).not.toHaveClass('nhsuk-tag--green');
    expect(tagElement).not.toHaveClass('nhsuk-tag--aqua-green');
    expect(tagElement).not.toHaveClass('nhsuk-tag--blue');
    expect(tagElement).not.toHaveClass('nhsuk-tag--purple');
    expect(tagElement).not.toHaveClass('nhsuk-tag--pink');
    expect(tagElement).not.toHaveClass('nhsuk-tag--red');
    expect(tagElement).not.toHaveClass('nhsuk-tag--orange');
    expect(tagElement).not.toHaveClass('nhsuk-tag--yellow');
  });
});
