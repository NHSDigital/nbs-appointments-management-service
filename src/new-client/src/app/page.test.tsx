import Home from './page';
import { render, screen } from '@testing-library/react';

describe('Home Page', () => {
  it('should render the layout', () => {
    render(<Home />);

    expect(screen.getByText('Hello world!')).toBeInTheDocument();
  });
});
