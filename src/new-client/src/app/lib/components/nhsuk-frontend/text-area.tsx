import { forwardRef, HTMLProps } from 'react';

type Props = {
  label: string;
} & HTMLProps<HTMLTextAreaElement>;
type Ref = HTMLTextAreaElement;

/**
 * A text area component adhering to the NHS UK Frontend design system.
 * Before making changes to this component, please consult the NHS UK Frontend documentation for it.
 * @see https://service-manual.nhs.uk/design-system/components/text-area
 */
export const TextArea = forwardRef<Ref, Props>((props, ref) => (
  <div className="nhsuk-form-group">
    <label className="nhsuk-label" htmlFor={props.id}>
      {props.label}
    </label>
    <textarea
      className="nhsuk-textarea"
      type="text"
      ref={ref}
      rows={props.rows ?? 5}
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      aria-label={props.label}
    />
  </div>
));
TextArea.displayName = 'TextArea';

export default TextArea;
