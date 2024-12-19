import render from '@testing/render';
import { screen } from '@testing-library/react';
import { BackLink } from '@nhsuk-frontend-components';

describe('Back Link', () => {
  it('renders', () => {
    render(
      <BackLink href="/mock-route" renderingStrategy="server" text="Go back" />,
    );

    expect(screen.getByText('Go back')).toBeInTheDocument();
  });

  it('renders with a link', () => {
    render(
      <BackLink href="/mock-route" renderingStrategy="server" text="Go back" />,
    );

    expect(screen.getByRole('link', { name: 'Go back' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Go back' })).toHaveAttribute(
      'href',
      '/mock-route',
    );
  });

  it('invokes the onClick handler when rendered in a client component', async () => {
    const onClick = jest.fn();
    const { user } = render(
      <BackLink onClick={onClick} renderingStrategy="client" text="Go back" />,
    );

    await user.click(screen.getByRole('link'));

    expect(onClick).toHaveBeenCalled();
  });
});
