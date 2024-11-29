import { render, screen } from '@testing-library/react';
import { WarningCallout } from '@nhsuk-frontend-components';

describe('Warning Callout', () => {
  it('renders', () => {
    render(
      <WarningCallout title="Test Warning Title">
        Test warning advice
      </WarningCallout>,
    );

    expect(
      screen.getByRole('heading', { name: 'Important: Test Warning Title' }),
    ).toBeInTheDocument();

    expect(screen.getByText('Test warning advice')).toBeInTheDocument();
  });

  it('defaults the title to "Important" if not provided', () => {
    render(<WarningCallout>Test warning advice</WarningCallout>);

    expect(
      screen.getByRole('heading', { name: 'Important' }),
    ).toBeInTheDocument();
  });

  it('does not include "Important" in the visually hidden text if the title already includes it', () => {
    render(
      <WarningCallout title="Important: Test Warning Title">
        Test warning advice
      </WarningCallout>,
    );

    expect(
      screen.getByRole('heading', { name: 'Important: Test Warning Title' }),
    ).toBeInTheDocument();
  });
});
