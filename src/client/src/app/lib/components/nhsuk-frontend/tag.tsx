import React from 'react';

export type TagColor =
  | 'white'
  | 'grey'
  | 'green'
  | 'aqua-green'
  | 'blue'
  | 'purple'
  | 'pink'
  | 'red'
  | 'orange'
  | 'yellow';

type Props = {
  text: string;
  color?: TagColor;
};

/**
 * An Tag component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/tag
 */
const Tag = ({ text, color }: Props) => {
  return (
    <strong className={`nhsuk-tag ${color ? `nhsuk-tag--${color}` : ''}`}>
      {text}
    </strong>
  );
};

export default Tag;
