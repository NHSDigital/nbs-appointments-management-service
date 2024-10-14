type SummaryListItem = {
  title: string;
  value: string;
  action?: {
    href: string;
    text: string;
  };
};

type Props = {
  items: SummaryListItem[];
  borders?: boolean;
};

/**
 * A summary list component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/summary-list
 */
const SummaryList = ({ items, borders = true }: Props) => {
  return (
    <dl
      className={`nhsuk-summary-list ${borders ? '' : 'nhsuk-summary-list--no-border'}`}
    >
      {items.map((item, index) => {
        return (
          <div
            className="nhsuk-summary-list__row"
            key={`summary-list-row-${index}`}
          >
            <dt className="nhsuk-summary-list__key" aria-label={item.title}>
              {item.title}
            </dt>
            <dd className="nhsuk-summary-list__value" aria-label={item.value}>
              {item.value}
            </dd>
            {item.action && (
              <dd
                className="nhsuk-summary-list__actions"
                aria-label={item.action.text}
              >
                <a href={item.action.href}>
                  <span className="nhsuk-u-visually-hidden">
                    {item.action.text}
                  </span>
                </a>
              </dd>
            )}
          </div>
        );
      })}
    </dl>
  );
};

export default SummaryList;
