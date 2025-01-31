import { forwardRef, HTMLProps } from 'react';

export type SelectOption = {
  value: string;
  label: string;
};

type Props = {
  label: string;
  options?: SelectOption[];
} & HTMLProps<HTMLSelectElement>;
type Ref = HTMLSelectElement;

/**
 * A select component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/select
 */
export const Select = forwardRef<Ref, Props>((props, ref) => (
  <div className="nhsuk-form-group">
    <label className="nhsuk-label" htmlFor={props.id}>
      {props.label}
    </label>

    <select
      className="nhsuk-select"
      ref={ref}
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
    >
      {props.options?.map((option, index) => {
        return (
          <option key={index} value={option.value}>
            {option.label}
          </option>
        );
      })}
      {props.children}
    </select>
  </div>
));
Select.displayName = 'Select';

export default Select;
