import { HTMLProps } from 'react';

type ButtonStyleType = 'primary' | 'secondary' | 'reverse' | 'warning';

type Props = HTMLProps<HTMLButtonElement> & {
  styleType?: ButtonStyleType;
  type?: 'submit' | 'reset' | 'button';
};

/**
 * A button component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/buttons
 */
const Button = ({
  styleType = 'primary',
  onClick,
  children,
  type = 'button',
  ...rest
}: Props) => {
  return (
    <button
      className={getClassForType(styleType)}
      data-module="nhsuk-button"
      onClick={onClick}
      type={type}
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...rest}
    >
      {children}
    </button>
  );
};

const getClassForType = (type: ButtonStyleType): string => {
  switch (type) {
    case 'secondary':
      return 'nhsuk-button nhsuk-button--secondary-solid';
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
