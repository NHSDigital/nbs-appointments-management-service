import React from 'react';
import { ReactNode, Children } from 'react';

type Props = {
  children: ReactNode;
  vertical?: boolean;
};

const ButtonGroup = ({ children, vertical = false }: Props) => {
  const childrenArray = Children.toArray(children);

  return (
    <ol
      className={`nhsuk-list nhsuk-u-clear nhsuk-u-margin-0 ${
        vertical ? 'flex-col' : 'flex-row'
      }`}
      style={{
        display: 'flex',
        flexDirection: vertical ? 'column' : 'row',
        gap: '0.5rem', // spacing between items
        padding: 0,
        margin: 0,
        listStyle: 'none',
      }}
    >
      {childrenArray.map((child, index) => {
        return (
          <li
            key={`button-list-${index}`}
            className={`nhsuk-a-to-z-min-width ${
              vertical ? '' : 'nhsuk-u-float-left nhsuk-u-margin-right-3'
            }`}
          >
            {child}
          </li>
        );
      })}
    </ol>
  );
};

export default ButtonGroup;
