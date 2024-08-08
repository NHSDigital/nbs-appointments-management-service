import { ReactNode } from 'react';

type Props = {
  title: string;
  children: ReactNode;
};

const NhsWarning = ({ title, children }: Props) => {
  return (
    <div className="nhsuk-warning-callout">
      <h3 className="nhsuk-warning-callout__label">
        <span>
          <span className="nhsuk-u-visually-hidden">Important: </span>
          {title}
        </span>
      </h3>
      {children}
    </div>
  );
};

export default NhsWarning;
