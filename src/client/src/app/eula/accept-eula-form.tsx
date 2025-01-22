'use client';
import { Button, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import { acceptEula } from '@services/appointmentsService';
import { EulaVersion } from '@types';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';

type AcceptEulaFormProps = {
  eulaVersion: EulaVersion;
};

export const AcceptEulaForm = ({ eulaVersion }: AcceptEulaFormProps) => {
  const { push } = useRouter();

  const {
    handleSubmit,
    formState: { isSubmitting, isSubmitSuccessful },
  } = useForm();

  const submitForm: SubmitHandler<Record<string, undefined>> = async () => {
    await acceptEula(eulaVersion.versionDate);
    push('/');
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button aria-label="Accept and continue" type="submit">
          Accept and continue
        </Button>
      )}
    </form>
  );
};
