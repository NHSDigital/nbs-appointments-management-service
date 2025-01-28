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
  {
    title: 'Phone Number',
    value: undefined,
  },
];

export const verifySummaryListItem = (term: string, expectedValue: string) => {
  const termRole = screen.getByRole('term', {
    name: `${term}-term`,
  });
  expect(termRole).toBeInTheDocument();
  expect(termRole).toHaveTextContent(term);
  const description = screen.getByRole('definition', {
    name: `${term}-description`,
  });
  expect(description).toBeInTheDocument();
  expect(description).toHaveTextContent(expectedValue);
};

describe('SummaryList', () => {
  it('renders', () => {
    render(<SummaryList items={mockItems} />);

    verifySummaryListItem('Name', 'John Doe');
  });

  it('renders actions if provided', () => {
    render(<SummaryList items={[mockItems[1]]} />);

    verifySummaryListItem('Address', '123 Fake Street');

    const addressAction = screen.getByRole('definition', {
      name: 'Address-description-action',
    });
    expect(addressAction).toBeInTheDocument();
    expect(addressAction).toHaveTextContent('Change');
    expect(addressAction.children[0]).toHaveAttribute('href', 'mock-link');
  });

  it('renders multiple addresses if provided', () => {
    render(<SummaryList items={[mockItems[0], mockItems[2]]} />);

    verifySummaryListItem('Name', 'John Doe');
    verifySummaryListItem('Address', '456 Fake Streetsecond address');

    const termDescription = screen.getByRole('definition', {
      name: 'Address-description',
    });
    expect(termDescription.children.length).toBe(2);

    const addressParts = mockItems[2].value ?? [];

    expect(termDescription.children[0]).toHaveTextContent(addressParts[0]);
    expect(termDescription.children[1]).toHaveTextContent(addressParts[1]);
  });

  it('renders undefined value as empty string', () => {
    render(<SummaryList items={[mockItems[3]]} />);

    verifySummaryListItem('Phone Number', '');
  });
});
