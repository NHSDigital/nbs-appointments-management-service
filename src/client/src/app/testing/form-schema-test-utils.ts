import { ObjectSchema, ValidationError } from 'yup';

export const getValidationErrorMessageOrTrue = async <T extends object>(
  schema: ObjectSchema<T>,
  values: T,
): Promise<string | true> => {
  try {
    await schema.validate(values);
    return true;
  } catch (error) {
    if (error instanceof ValidationError) {
      return error.message;
    } else {
      throw error;
    }
  }
};
