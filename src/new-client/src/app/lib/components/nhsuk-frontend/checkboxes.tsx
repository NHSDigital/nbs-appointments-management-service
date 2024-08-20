export type CheckBoxProps = {
  fieldValue: string;
  fieldId: string;
  fieldName: string;
  prompt: string;
};

type Props = {
  checkboxes: CheckBoxProps[];
};

const CheckBoxes = ({ checkboxes }: Props) => {
  return (
    <div className="nhsuk-checkboxes">
      {checkboxes.map((checkbox, index) => {
        return (
          <CheckBox
            key={`checkbox-item-${index}`}
            fieldValue={checkbox.fieldValue}
            fieldId={checkbox.fieldId}
            fieldName={checkbox.fieldName}
            prompt={checkbox.prompt}
          />
        );
      })}
    </div>
  );
};

const CheckBox = ({
  fieldValue,
  fieldId,
  fieldName,
  prompt,
}: CheckBoxProps) => {
  return (
    <div className="nhsuk-checkboxes__item">
      <input
        className="nhsuk-checkboxes__input"
        id={fieldId}
        name={fieldName}
        type="checkbox"
        value={fieldValue}
      />
      <label className="nhsuk-label nhsuk-checkboxes__label" htmlFor={fieldId}>
        {prompt}
      </label>
    </div>
  );
};

export default CheckBoxes;
