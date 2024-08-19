import { render, screen } from '@testing-library/react';
import { When } from '@components/when';

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
});
