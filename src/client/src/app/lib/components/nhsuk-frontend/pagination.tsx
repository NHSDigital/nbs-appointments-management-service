import Link from 'next/link';

export type PaginationLink = {
  title: string;
  href: string;
};

type PaginationProps = {
  previous: PaginationLink | null;
  next: PaginationLink | null;
};

/**
 * A pagination component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/pagination
 */
const Pagination = ({ previous, next }: PaginationProps) => {
  return (
    <nav
      className="nhsuk-pagination"
      role="navigation"
      aria-label="Pagination"
      style={{ marginTop: 0, marginBottom: 0 }}
    >
      <ul className="nhsuk-list nhsuk-pagination__list">
        {previous !== null && (
          <li className="nhsuk-pagination-item--previous">
            <Link
              className="nhsuk-pagination__link nhsuk-pagination__link--prev"
              href={previous.href}
            >
              <span className="nhsuk-pagination__title">Previous</span>
              <span className="nhsuk-u-visually-hidden">:</span>
              <span className="nhsuk-pagination__page">{previous.title}</span>
              <svg
                className="nhsuk-icon nhsuk-icon__arrow-left"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                aria-hidden="true"
                width="34"
                height="34"
              >
                <path d="M4.1 12.3l2.7 3c.2.2.5.2.7 0 .1-.1.1-.2.1-.3v-2h11c.6 0 1-.4 1-1s-.4-1-1-1h-11V9c0-.2-.1-.4-.3-.5h-.2c-.1 0-.3.1-.4.2l-2.7 3c0 .2 0 .4.1.6z"></path>
              </svg>
            </Link>
          </li>
        )}
        {next !== null && (
          <li className="nhsuk-pagination-item--next">
            <Link
              className="nhsuk-pagination__link nhsuk-pagination__link--next"
              href={next.href}
            >
              <span className="nhsuk-pagination__title">Next</span>
              <span className="nhsuk-u-visually-hidden">:</span>
              <span className="nhsuk-pagination__page">{next.title}</span>
              <svg
                className="nhsuk-icon nhsuk-icon__arrow-right"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                aria-hidden="true"
                width="34"
                height="34"
              >
                <path d="M19.6 11.66l-2.73-3A.51.51 0 0 0 16 9v2H5a1 1 0 0 0 0 2h11v2a.5.5 0 0 0 .32.46.39.39 0 0 0 .18 0 .52.52 0 0 0 .37-.16l2.73-3a.5.5 0 0 0 0-.64z"></path>
              </svg>
            </Link>
          </li>
        )}
      </ul>
    </nav>
  );
};

export default Pagination;
