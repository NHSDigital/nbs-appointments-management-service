import render from '@testing/render';
import { screen, within } from '@testing-library/react';
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

export const verifySummaryListItem = (
  term: string,
  expectedValue: string | string[],
) => {
  const matchingListItem = screen.getByRole('listitem', {
    name: `${term} summary`,
  });

  const termElement = within(matchingListItem).getByRole('term', {
    name: term,
  });
  expect(termElement).toBeInTheDocument();
  expect(termElement).toHaveTextContent(term);

  if (typeof expectedValue === 'string') {
    const definitionElement = within(matchingListItem).getByRole('definition', {
      name: expectedValue,
    });
    expect(definitionElement).toBeInTheDocument();
    expect(definitionElement).toHaveTextContent(expectedValue);
  } else {
    const definitionElement = within(matchingListItem).getByRole('definition', {
      name: expectedValue.join(', '),
    });
    expect(definitionElement).toBeInTheDocument();
    expectedValue.forEach(value => {
      expect(definitionElement).toHaveTextContent(value);
    });
  }
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
      name: 'Change',
    });
    expect(addressAction).toBeInTheDocument();
    expect(addressAction).toHaveTextContent('Change');
    expect(addressAction.children[0]).toHaveAttribute('href', 'mock-link');
  });

  it('renders multiple addresses if provided', () => {
    render(
      <SummaryList
        items={[
          mockItems[0],
          {
            title: 'Address',
            value: ['456 Fake Street', 'Mock City'],
          },
        ]}
      />,
    );

    verifySummaryListItem('Name', 'John Doe');
    verifySummaryListItem('Address', ['456 Fake Street', 'Mock City']);

    const termDescription = screen.getByRole('definition', {
      name: '456 Fake Street, Mock City',
    });
    expect(termDescription.children.length).toBe(2);

    const addressParts = ['456 Fake Street', 'Mock City'];

    expect(termDescription.children[0]).toHaveTextContent(addressParts[0]);
    expect(termDescription.children[1]).toHaveTextContent(addressParts[1]);
  });

  it('renders undefined value as empty string', () => {
    render(<SummaryList items={[mockItems[3]]} />);

    verifySummaryListItem('Phone Number', '');
  });
});
