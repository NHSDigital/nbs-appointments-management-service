import render from '@testing/render';
import { screen } from '@testing-library/react';
import { InsetText } from '@nhsuk-frontend-components';

describe('InsetText', () => {
  it('renders', () => {
    render(<InsetText>This is some inset text</InsetText>);

    expect(screen.getByRole('paragraph')).toBeInTheDocument();
  });

  it('renders child text', () => {
    render(<InsetText>This is some inset text</InsetText>);

    expect(screen.getByText('This is some inset text')).toBeInTheDocument();
  });
});
