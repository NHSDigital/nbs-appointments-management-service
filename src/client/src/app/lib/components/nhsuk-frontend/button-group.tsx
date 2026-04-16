import React, { ReactNode, Children } from 'react';

type Props = {
  children: ReactNode;
  vertical?: boolean;
};

const ButtonGroup = ({ children, vertical = false }: Props) => {
  return (
    <ol
      className={`nhsuk-list nhsuk-u-margin-0 nhsuk-button-group-flat ${
        vertical ? 'flex-col' : 'flex-row'
      }`}
      style={{
        display: 'flex',
        flexDirection: vertical ? 'column' : 'row',
        gap: '1rem',
        padding: 0,
        margin: 0,
        listStyle: 'none',
        alignItems: vertical ? 'stretch' : 'center',
      }}
    >
      {Children.toArray(children).map((child, index) => (
        <li key={index} className="nhsuk-u-margin-0">
          {child}
        </li>
      ))}
    </ol>
  );
};

export default ButtonGroup;
