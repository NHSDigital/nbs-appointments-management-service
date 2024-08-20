import { render, screen } from '@testing-library/react';
import { Card } from '@nhsuk-frontend-components';

describe('Card', () => {
  it('renders', () => {
    render(<Card title="Test title" />);

    expect(
      screen.getByRole('heading', { name: 'Test title' }),
    ).toBeInTheDocument();
  });

  it('renders a clickable card if a href is provided', () => {
    render(<Card title="Test title" href="/test" />);

    expect(
      screen.getByRole('link', { name: 'Test title' }),
    ).toBeInTheDocument();
  });

  it('renders a card with a description if provided', () => {
    render(<Card title="Test title" description="Test description" />);

    expect(screen.getByText('Test description')).toBeInTheDocument();
  });

  it('renders children', () => {
    render(
      <Card title="Test title">
        <div>This is a child component</div>
      </Card>,
    );

    expect(screen.getByText('This is a child component')).toBeInTheDocument();
  });
});
