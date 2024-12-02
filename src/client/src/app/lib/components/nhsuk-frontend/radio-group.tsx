import { Children, ReactNode } from 'react';

/**
 * A radio input component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/radios
 */
const RadioGroup = ({ children }: { children: ReactNode }) => {
  const childrenArray = Children.toArray(children);
  return (
    <div className="nhsuk-radios">
      {childrenArray.map((child, index) => {
        return (
          <div className="nhsuk-radios__item" key={`radio-group-item-${index}`}>
            {child}
          </div>
        );
      })}
    </div>
  );
};

export default RadioGroup;
