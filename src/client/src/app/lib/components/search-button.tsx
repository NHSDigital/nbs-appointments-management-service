import { HTMLProps } from 'react';

type Props = HTMLProps<HTMLButtonElement> & {
  type?: 'submit' | 'reset' | 'button';
};

const SearchButton = ({ onClick, children, ...rest }: Props) => {
  return (
    <button
      className="nhsuk-button nhsuk-button--secondary-solid search-button"
      data-module="nhsuk-button"
      onClick={onClick}
      type="button"
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...rest}
    >
      {children}
    </button>
  );
};

export default SearchButton;
