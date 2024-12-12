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
  {
    title: 'Address',
    value: ['456 Fake Street', 'second address'],
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

  it('renders multiple addresses if provided', () => {
    render(<SummaryList items={mockItems} />);

    expect(screen.getByRole('term', { name: 'Name' })).toBeInTheDocument();
    expect(
      screen.getByRole('definition', { name: 'John Doe' }),
    ).toBeInTheDocument();

    const labelText = Array.isArray(mockItems[2].value)
      ? mockItems[2].value.join('')
      : mockItems[2].value;
    const addressEl = screen.getByLabelText(labelText);

    expect(addressEl).toBeInTheDocument();
    expect(addressEl.children.length).toBe(2);
    expect(addressEl.children[0]).toHaveTextContent(mockItems[2].value[0]);
    expect(addressEl.children[1]).toHaveTextContent(mockItems[2].value[1]);
  });
});
