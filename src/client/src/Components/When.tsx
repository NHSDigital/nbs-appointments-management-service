import React from "react";

export const When = (props: { condition: boolean; children: React.ReactNode }) => (
  <>{props.condition && props.children}</>
);
