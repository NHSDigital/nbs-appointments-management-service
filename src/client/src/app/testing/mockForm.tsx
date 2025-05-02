import { ReactElement, ReactNode } from 'react';
import * as yup from 'yup';
import {
  DefaultValues,
  FieldValues,
  FormProvider,
  SubmitHandler,
  useForm,
} from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';

type MockFormProps<T extends FieldValues> = {
  children: ReactNode;
  submitHandler: SubmitHandler<T>;
  defaultValues?: DefaultValues<T>;
  schema?: yup.ObjectSchema<T>;
};

const MockForm = <T extends FieldValues>({
  children,
  submitHandler,
  defaultValues,
  schema,
}: MockFormProps<T>): ReactElement => {
  const methods = useForm<T>({
    defaultValues: defaultValues,
    // TODO: Fix this type issue, Resolver<T> should work
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: schema ? (yupResolver(schema) as any) : undefined,
  });

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
