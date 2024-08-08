import { render, screen } from '@testing-library/react';
import { When } from './when';

describe('<When>', () => {
  it('renders its children when the condition is met', async () => {
    render(
      <When condition={true}>
        <div>content</div>
      </When>,
    );
    expect(screen.queryByText('content')).toBeVisible();
  });

  it('does not render its children when the condition is not met', async () => {
    render(
      <When condition={false}>
        <div>content</div>
      </When>,
    );
    expect(screen.queryByText('content')).toBeNull();
  });

  it('fails deliberately to check if the github action reports failure correctly', () => {
    expect(true).toBe(false);
  });
});
