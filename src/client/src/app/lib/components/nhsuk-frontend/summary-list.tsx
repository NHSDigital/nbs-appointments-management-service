import Link from 'next/link';

type LinkActionProps = {
  renderingStrategy: 'server';
  text: string;
  href: string;
};

type OnClickActionProps = {
  renderingStrategy: 'client';
  text: string;
  onClick: () => void;
};

export type SummaryListItem = {
  title: string;
  value: string | string[];
  action?: LinkActionProps | OnClickActionProps;
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
      role="list"
    >
      {items.map((item, index) => {
        return (
          <div
            className="nhsuk-summary-list__row"
            role="listitem"
            aria-label={`${item.title}-listitem`}
            key={`summary-list-row-${index}`}
          >
            <dt
              className="nhsuk-summary-list__key"
              aria-label={`${item.title}-term`}
            >
              {item.title}
            </dt>
            {typeof item.value === 'string' ? (
              <dd
                className="nhsuk-summary-list__value"
                aria-label={`${item.title}-description`}
              >
                {item.value}
              </dd>
            ) : (
              <dd
                className="nhsuk-summary-list__value"
                aria-label={`${item.title}-description`}
              >
                {item.value.map((i, valueIndex) => (
                  <div key={valueIndex}>{i}</div>
                ))}
              </dd>
            )}

            {item.action && (
              <dd
                className="nhsuk-summary-list__actions"
                aria-label={`${item.title}-description-action`}
              >
                {item.action.renderingStrategy === 'server' ? (
                  <Link href={item.action.href}>{item.action.text}</Link>
                ) : (
                  <Link href={''} onClick={item.action.onClick}>
                    {item.action.text}
                  </Link>
                )}
              </dd>
            )}
          </div>
        );
      })}
    </dl>
  );
};

export default SummaryList;
