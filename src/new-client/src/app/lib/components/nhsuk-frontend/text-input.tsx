type Props = {
  fieldId: string;
  fieldName: string;
  prompt: string;
};

const TextInput = ({ prompt, fieldId, fieldName }: Props) => {
  return (
    <div className="nhsuk-form-group">
      <label className="nhsuk-label" htmlFor={fieldId}>
        {prompt}
      </label>
      <input
        className="nhsuk-input"
        id={fieldId}
        name={fieldName}
        type="text"
      />
    </div>
  );
};

export default TextInput;
