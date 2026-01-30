import { ReactNode, MouseEventHandler } from 'react';
import RightChevron from '@components/nhsuk-frontend/icons/right-chevron';
import Link from 'next/link';

type CardType = 'primary' | 'secondary' | 'feature';

type Props = {
  title: string;
  type?: CardType;
  description?: string;
  href?: string;
  children?: ReactNode;
  actionLinks?: ReactNode;
  maxWidth?: number;
  onClick?: MouseEventHandler<HTMLAnchorElement>;
};

/**
 * A card component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/card
 */
const Card = ({
  title,
  type = 'primary',
  description,
  href,
  children,
  actionLinks,
  maxWidth,
  onClick,
}: Props) => {
  const isClickable = !!href || !!onClick;

  return (
    <div
      className={`nhsuk-card nhsuk-card--${type} ${isClickable ? 'nhsuk-card--clickable' : ''}`}
      style={maxWidth ? { maxWidth } : undefined}
    >
      <div className={`nhsuk-card__content nhsuk-card__content--${type}`}>
        <div className="card-title-wrapper">
          <h2 className="nhsuk-card__heading nhsuk-heading-m">
            {href ? (
              <Link className="nhsuk-card__link" href={href} onClick={onClick}>
                {title}
              </Link>
            ) : (
              title
            )}
          </h2>
          {actionLinks ? (
            <div className="card-action-link">{actionLinks}</div>
          ) : null}
        </div>

        {description ? (
          <p className="nhsuk-card__description">{description}</p>
        ) : null}

        {children ? children : null}

        {isClickable ? <RightChevron /> : null}
      </div>
    </div>
  );
};

export default Card;
