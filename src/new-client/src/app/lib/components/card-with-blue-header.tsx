import { Children, ReactNode } from 'react';

type Props = {
  title: string;
  children: ReactNode;
};

const CardWithBlueHeader = ({ title, children }: Props) => {
  const childrenArray = Children.toArray(children);

  return (
    <div className="nhsuk-card nhsuk-card--care nhsuk-card--care--non-urgent">
      <div className="nhsuk-card--care__heading-container">
        <h3 className="nhsuk-card--care__heading">
          <span>
            <span className="nhsuk-u-visually-hidden">Non-urgent advice: </span>
            {title}
          </span>
        </h3>
      </div>

      {childrenArray.map((child, index) => {
        const className =
          index === 0
            ? 'nhsuk-card__content'
            : 'nhsuk-card-custom__content-row';
        return (
          <div className={className} style={{ borderTop: 3 }} key={index}>
            {child}
          </div>
        );
      })}
    </div>
  );
};

export default CardWithBlueHeader;
