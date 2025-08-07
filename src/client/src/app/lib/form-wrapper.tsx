import { ReactNode, useEffect } from 'react';
import {
  FieldErrors,
  FieldValues,
  SubmitHandler,
  UseFormHandleSubmit,
  UseFormSetError,
} from 'react-hook-form';

type FormWrapperProps<T extends FieldValues> = {
  submitHandler: (formPayload: T) => void;
  handleSubmit: UseFormHandleSubmit<T, T>;
  setError: UseFormSetError<T>;
  errors: FieldErrors<T>;
  children: ReactNode;
};

const FormWrapper = <T extends FieldValues>({
  submitHandler,
  handleSubmit,
  setError,
  errors,
  children,
}: FormWrapperProps<T>) => {
  const submitForm: SubmitHandler<T> = async (form: T) => {
    try {
      submitHandler(form);
    } catch (error) {
      setError('root', {
        message: 'An error occurred while submitting the form.',
      });
    }
  };

  useEffect(() => {
    if (errors.root !== undefined) {
      throw new Error('Form submission error.');
    }
  });

  return <form onSubmit={handleSubmit(submitForm)}>{children}</form>;
};

export default FormWrapper;
