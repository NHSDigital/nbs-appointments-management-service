import React from 'react';

type TagColour =
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
  colour: TagColour;
};

/**
 * An Tag component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/tag
 */
const Tag = ({ text, colour }: Props) => {
  return <strong className={`nhsuk-tag nhsuk-tag--${colour}`}>{text}</strong>;
};

export default Tag;
