import render from '@testing/render';
import { screen } from '@testing-library/react';
import NhsHeading from './nhs-heading';

describe('NHS Heading', () => {
  it('renders', () => {
    render(<NhsHeading title="Mock title" caption="Mock caption" />);

    expect(
      screen.getByRole('heading', { name: 'Mock caption Mock title' }),
    ).toBeInTheDocument();
  });
});
