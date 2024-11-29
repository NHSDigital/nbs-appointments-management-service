import React from 'react';
import { ReactNode, Children } from 'react';

type Props = {
  children: ReactNode;
};

const ButtonGroup = ({ children }: Props) => {
  const childrenArray = Children.toArray(children);

  return (
    <ol className="nhsuk-list nhsuk-u-clear nhsuk-u-margin-0">
      {childrenArray.map((child, index) => {
        return (
          <li
            key={`button-list-${index}`}
            className="nhsuk-u-margin-bottom-0 nhsuk-u-float-left nhsuk-u-margin-right-3 nhsuk-a-to-z-min-width"
          >
            {child}
          </li>
        );
      })}
    </ol>
  );
};

export default ButtonGroup;
