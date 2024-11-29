import { ReactElement, ReactNode } from 'react';
import {
  DefaultValues,
  FieldValues,
  FormProvider,
  SubmitHandler,
  useForm,
} from 'react-hook-form';

type MockFormProps<T extends FieldValues> = {
  children: ReactNode;
  submitHandler: SubmitHandler<T>;
  defaultValues?: DefaultValues<T>;
};

const MockForm = <T extends FieldValues>({
  children,
  submitHandler,
  defaultValues,
}: MockFormProps<T>): ReactElement => {
  const methods = useForm<T>({ defaultValues: defaultValues });

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
