import { render, screen } from '@testing-library/react';
import { Header } from '@nhsuk-frontend-components';

describe('Header', () => {
  it('renders', () => {
    render(<Header />);

    expect(screen.getByRole('banner')).toBeInTheDocument();
  });

  it('contains a link back to the home page', () => {
    render(<Header />);

    expect(
      screen.getByRole('link', { name: 'NHS Appointment Book' }),
    ).toBeInTheDocument();
  });

  it('renders children', () => {
    render(
      <Header>
        <div>Some child content</div>
      </Header>,
    );

    expect(screen.getByText('Some child content')).toBeInTheDocument();
  });
});
