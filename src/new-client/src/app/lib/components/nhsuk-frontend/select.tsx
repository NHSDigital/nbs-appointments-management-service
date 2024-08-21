import { HTMLProps } from 'react';

type Option = {
  value: string;
  label: string;
};

type Props = HTMLProps<HTMLSelectElement> & {
  options: Option[];
};

/**
 * A select component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/select
 */
const Select = ({ options, ...rest }: Props) => {
  return (
    <div className="nhsuk-form-group">
      <label className="nhsuk-label" htmlFor="select-1">
        Label text goes here
      </label>

      <select
        className="nhsuk-select"
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...rest}
      >
        {options.map((option, index) => {
          return (
            <option key={index} value={option.value}>
              {option.label}
            </option>
          );
        })}
      </select>
    </div>
  );
};

export default Select;
