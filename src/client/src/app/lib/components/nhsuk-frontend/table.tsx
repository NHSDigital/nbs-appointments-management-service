import { ReactNode } from 'react';

type TableProps = {
  caption?: string;
  headers: Cell[];
  rows: Cell[][];
};

export type Cell = string | ReactNode;

type TableCardinality = 'two-column' | 'three-or-more-column';

/**
 * A table component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/table
 */
const Table = ({ caption, headers, rows }: TableProps) => {
  const cardinality =
    headers.length > 2 ? 'three-or-more-column' : 'two-column';

  return (
    <table
      className={
        cardinality === 'two-column' ? 'nhsuk-table' : 'nhsuk-table-responsive'
      }
    >
      {caption !== undefined && (
        <caption className="nhsuk-table__caption">{caption}</caption>
      )}
      <thead className="nhsuk-table__head">
        <tr>
          {headers.map((header, index) => {
            return (
              <th className="" scope="col" key={`header-${index}`}>
                {header}
              </th>
            );
          })}
        </tr>
      </thead>

      <tbody className="nhsuk-table__body">
        {rows.map((row, index) => {
          return renderRow(row, cardinality, headers, index);
        })}
      </tbody>
    </table>
  );
};

const renderRow = (
  row: Cell[],
  cardinality: TableCardinality,
  headers: Cell[],
  rowIndex: number,
) => {
  return (
    <tr className="nhsuk-table__row" key={`row-${rowIndex}`}>
      {row.map((cell, cellIndex) => {
        return (
          <td
            className="nhsuk-table__cell"
            key={`row-${rowIndex}-cell-${cellIndex}`}
          >
            {cardinality === 'three-or-more-column' && (
              <span className="nhsuk-table-responsive__heading" aria-hidden>
                {headers[cellIndex]}
              </span>
            )}
            {cell}
          </td>
        );
      })}
    </tr>
  );
};

export default Table;
