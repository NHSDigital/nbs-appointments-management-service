import { render, screen, within } from '@testing-library/react';
import { Table } from '@nhsuk-frontend-components';

const mockCaption = 'This is a mock table caption.';
const mockHeaders = ['Header 1', 'Header 2'];
const mockRows = [
  ['Row 1 Col 1', 'Row 1 Col 2'],
  ['Row 2 Col 1', 'Row 2 Col 2'],
];

describe('<Table />', () => {
  it('renders', () => {
    render(
      <Table caption={mockCaption} headers={mockHeaders} rows={mockRows} />,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('renders a caption linked to the table', () => {
    render(
      <Table caption={mockCaption} headers={mockHeaders} rows={mockRows} />,
    );

    expect(screen.getByRole('table')).toHaveAccessibleName(mockCaption);
    expect(screen.getByRole('caption')).toHaveTextContent(mockCaption);
  });

  it('renders each header and row', () => {
    render(
      <Table caption={mockCaption} headers={mockHeaders} rows={mockRows} />,
    );

    mockHeaders.forEach(header => {
      expect(
        screen.getByRole('columnheader', { name: header }),
      ).toBeInTheDocument();
    });

    expect(screen.getAllByRole('row')).toHaveLength(mockRows.length + 1);

    mockRows.forEach(row => {
      row.forEach(cell => {
        expect(screen.getByRole('cell', { name: cell })).toBeInTheDocument();
      });
    });
  });

  it('uses the correct NHS.UK Frontend class for tables with only 2 columns', () => {
    render(
      <Table caption={mockCaption} headers={mockHeaders} rows={mockRows} />,
    );

    expect(screen.getByRole('table')).toHaveClass('nhsuk-table');
  });

  it.each([3, 4, 10])(
    'uses the correct NHS.UK Frontend class for tables with 3 or more columns',
    (numOfHeaders: number) => {
      render(
        <Table
          caption={mockCaption}
          headers={getMockHeaders(numOfHeaders)}
          rows={getMockRows(numOfHeaders, 2)}
        />,
      );

      expect(screen.getByRole('table')).toHaveClass('nhsuk-table-responsive');
    },
  );

  it('adds required "aria-hidden" span for tables with 3 or more columns', () => {
    render(
      <Table
        caption={mockCaption}
        headers={getMockHeaders(4)}
        rows={getMockRows(4, 2)}
      />,
    );

    const firstRow = screen.getAllByRole('row')[1];
    within(firstRow)
      .getAllByRole('cell')
      .forEach((cell, index) => {
        const hiddenSpan = within(cell).getByText(`Header ${index + 1}`);
        expect(hiddenSpan).toHaveClass('nhsuk-table-responsive__heading');
        expect(hiddenSpan).toHaveAttribute('aria-hidden');
      });
  });
});

const getMockHeaders = (columns: number) => {
  return Array.from({ length: columns }, (_, index) => `Header ${index + 1}`);
};

const getMockRows = (columns: number, rows: number) => {
  return Array.from({ length: rows }, (_, rowIndex) => {
    return Array.from({ length: columns }, ($, columnIndex) => {
      return `Row ${rowIndex + 1} Col ${columnIndex + 1}`;
    });
  });
};
