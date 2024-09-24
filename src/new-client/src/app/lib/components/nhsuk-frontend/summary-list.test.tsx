import render from '@testing/render';
import { screen } from '@testing-library/react';
import { SummaryList } from '@nhsuk-frontend-components';

const mockItems = [
  { title: 'Name', value: 'John Doe' },
  {
    title: 'Address',
    value: '123 Fake Street',
    action: { href: '/user/john-doe/change-address', text: 'Change' },
  },
];

describe('SummaryList', () => {
  it('renders', () => {
    render(<SummaryList items={mockItems} />);

    expect(screen.getByRole('term', { name: 'Name' })).toBeInTheDocument();
    expect(
      screen.getByRole('definition', { name: 'John Doe' }),
    ).toBeInTheDocument();
  });

  it('renders actions if provided', () => {
    render(<SummaryList items={mockItems} />);

    expect(
      screen.getByRole('definition', { name: 'Change' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Change' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Change' })).toHaveAttribute(
      'href',
      '/user/john-doe/change-address',
    );
  });
});
