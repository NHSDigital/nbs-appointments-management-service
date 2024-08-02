import { ReactNode } from 'react';

type Props = {
  title: string;
  children: ReactNode;
};

const NhsCard = ({ title, children }: Props) => (
  <div className="nhsuk-card nhsuk-card--feature">
    <div className="nhsuk-card__content nhsuk-card__content--feature">
      <h2 className="nhsuk-card__heading nhsuk-card__heading--feature nhsuk-u-font-size-24">
        {title}
      </h2>
      {children}
    </div>
  </div>
);

export default NhsCard;
