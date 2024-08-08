import React from 'react';

type WhenProps = {
  condition: boolean;
  children: React.ReactNode;
};

export const When = (props: WhenProps) => (
  <>{props.condition && props.children}</>
);
