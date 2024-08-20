import { HTMLProps } from 'react';

type ButtonType = 'primary' | 'secondary' | 'reverse' | 'warning';

type Props = HTMLProps<HTMLButtonElement> & {
  type?: ButtonType;
};

/**
 * A button component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/buttons
 */
const Button = ({ type = 'primary', onClick, children }: Props) => {
  return (
    <button
      className={getClassForType(type)}
      data-module="nhsuk-button"
      type="submit"
      onClick={onClick}
    >
      {children}
    </button>
  );
};

const getClassForType = (type: ButtonType): string => {
  switch (type) {
    case 'secondary':
      return 'nhsuk-button nhsuk-button--secondary';
    case 'reverse':
      return 'nhsuk-button nhsuk-button--reverse';
    case 'warning':
      return 'nhsuk-button nhsuk-button--warning';
    case 'primary':
    default:
      return 'nhsuk-button';
  }
};

export default Button;
