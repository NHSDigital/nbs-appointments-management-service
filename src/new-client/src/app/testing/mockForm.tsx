import { ReactElement, ReactNode } from 'react';
import {
  FieldValues,
  FormProvider,
  SubmitHandler,
  useForm,
} from 'react-hook-form';

type MockFormProps<T extends FieldValues> = {
  children: ReactNode;
  submitHandler: SubmitHandler<T>;
};

const MockForm = <T extends FieldValues>({
  children,
  submitHandler,
}: MockFormProps<T>): ReactElement => {
  const methods = useForm<T>();

  const submitForm: SubmitHandler<T> = (formData: T) => {
    submitHandler(formData);
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(submitForm)}>{children}</form>
    </FormProvider>
  );
};

export default MockForm;
