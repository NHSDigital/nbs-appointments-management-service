import render from '@testing/render';
import { screen } from '@testing-library/react';
import { SummaryList, SummaryListItem } from '@nhsuk-frontend-components';

const mockItems: SummaryListItem[] = [
  { title: 'Name', value: 'John Doe' },
  {
    title: 'Address',
    value: '123 Fake Street',
    action: {
      href: 'mock-link',
      text: 'Change',
      renderingStrategy: 'server',
    },
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
      'mock-link',
    );
  });
});
