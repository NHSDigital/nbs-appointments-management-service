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
  PathValue,
} from 'react-hook-form';

const PHONE_NUMBER_REGEX = new RegExp(/^[0-9 ]*$/);

interface PhoneNumberFormControlProps<
  TFieldValues extends FieldValues,
  TContext,
> {
  formField: FieldPath<TFieldValues>;
  label: string;
  control: Control<TFieldValues, TContext>;
  errors: FieldErrors<TFieldValues>;
}

const PhoneNumberFormControl = <TFieldValues extends FieldValues, TContext>({
  formField,
  label,
  control,
  errors,
}: PhoneNumberFormControlProps<TFieldValues, TContext>): JSX.Element => {
  const onInputChange = (
    e: ChangeEvent<HTMLInputElement>,
    field: ControllerRenderProps<TFieldValues, Path<TFieldValues>>,
  ) => {
    if (PHONE_NUMBER_REGEX.test(e.target.value)) {
      field.onChange(e.target.value);
    }
  };

  const validateInput = (
    value: PathValue<TFieldValues, Path<TFieldValues>>,
  ) => {
    if (value.trim().length === 0) {
      return 'Enter a value';
    }

    if (!PHONE_NUMBER_REGEX.test(value)) {
      return 'Enter a valid phone number';
    }
  };

  return (
    <FormGroup error={(errors[formField]?.message as string) ?? ''}>
      <Controller
        name={formField}
        control={control}
        rules={{
          validate: value => validateInput(value),
        }}
        render={({ field }) => (
          <TextInput
            {...field}
            id={formField}
            type="tel"
            label={label}
            onChange={e => onInputChange(e, field)}
          />
        )}
      />
    </FormGroup>
  );
};

export default PhoneNumberFormControl;
