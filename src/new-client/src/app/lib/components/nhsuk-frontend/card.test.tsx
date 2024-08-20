import { render, screen } from '@testing-library/react';
import { Card } from '@nhsuk-frontend-components';

describe('Card', () => {
  it('renders', () => {
    render(
      <Card title="Test title">
        <div>This is a child component</div>
      </Card>,
    );

    expect(
      screen.getByRole('heading', { name: 'Test title' }),
    ).toBeInTheDocument();
    expect(screen.getByText('This is a child component')).toBeInTheDocument();
  });

  // TODO: Write more tests
});
