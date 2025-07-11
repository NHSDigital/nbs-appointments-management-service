import SearchButton from '@components/search-button';
import { render } from '@testing-library/react';
import { screen } from '@testing-library/react';

describe('Search Button', () => {
  it('renders', () => {
    render(<SearchButton>Click me!</SearchButton>);

    expect(
      screen.getByRole('button', { name: 'Click me!' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Click me!' })).toHaveAttribute(
      'class',
      'nhsuk-button nhsuk-button--secondary-solid app-button--small',
    );
  });
});
