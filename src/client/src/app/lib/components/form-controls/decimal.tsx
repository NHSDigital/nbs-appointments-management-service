import { FormGroup, TextInput } from '@components/nhsuk-frontend';
import { ChangeEvent } from 'react';
import {
  Control,
  Controller,
  ControllerRenderProps,
  FieldErrors,
  FieldPath,
  FieldValues,
  Path,
} from 'react-hook-form';

//the string only contains: numbers, '-', '.'; or is empty
const INPUT_DECIMAL_REGEX = new RegExp(/^[-\d.]*$/);

interface DecimalFormControlProps<TFieldValues extends FieldValues, TContext> {
  formField: FieldPath<TFieldValues>;
  label: string;
  control: Control<TFieldValues, TContext>;
  errors: FieldErrors<TFieldValues>;
}

const DecimalFormControl = <TFieldValues extends FieldValues, TContext>({
  formField,
  label,
  control,
  errors,
}: DecimalFormControlProps<TFieldValues, TContext>): JSX.Element => {
  const onInputChange = (
    e: ChangeEvent<HTMLInputElement>,
    field: ControllerRenderProps<TFieldValues, Path<TFieldValues>>,
  ) => {
    if (INPUT_DECIMAL_REGEX.test(e.target.value)) {
      field.onChange(e.target.value);
    }
  };

  return (
    <FormGroup error={(errors[formField]?.message as string) ?? ''}>
      <Controller
        name={formField}
        control={control}
        render={({ field }) => (
          <TextInput
            {...field}
            id={formField}
            label={label}
            onChange={e => onInputChange(e, field)}
          />
        )}
      />
    </FormGroup>
  );
};

export default DecimalFormControl;
