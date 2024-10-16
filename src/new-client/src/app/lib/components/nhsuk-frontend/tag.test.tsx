import render from '@testing/render';
import { screen } from '@testing-library/react';
import { Tag } from '@nhsuk-frontend-components';

describe('Tag', () => {
  it('renders', () => {
    render(<Tag text="Test" colour="white" />);

    expect(screen.getByText('Test')).toBeInTheDocument();
  });

  it('uses the strong role', () => {
    render(<Tag text="Test" colour="white" />);

    expect(screen.getByRole('strong')).toBeInTheDocument();
  });
});
