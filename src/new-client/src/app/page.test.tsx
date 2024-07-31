import Home from './page';
import { render, screen } from '@testing-library/react';

describe('Home Page', () => {
  it('should render the home page', () => {
    render(<Home />);

    expect(
      screen.getByText(
        'Welcome to the National Booking Service - Appointment Management System',
      ),
    ).toBeInTheDocument();
  });
});
