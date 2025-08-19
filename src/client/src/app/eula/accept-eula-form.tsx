'use client';
import { Button, SmallSpinnerWithText } from '@components/nhsuk-frontend';
import { acceptEula } from '@services/appointmentsService';
import { EulaVersion } from '@types';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';
import { useTransition } from 'react';

type AcceptEulaFormProps = {
  eulaVersion: EulaVersion;
};

export const AcceptEulaForm = ({ eulaVersion }: AcceptEulaFormProps) => {
  const [pendingSubmit, startTransition] = useTransition();
  const { push } = useRouter();

  const { handleSubmit } = useForm();

  const submitForm: SubmitHandler<Record<string, undefined>> = async () => {
    startTransition(async () => {
      await acceptEula(eulaVersion.versionDate);
      push('/sites');
    });
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      {pendingSubmit ? (
        <SmallSpinnerWithText text="Working..." />
      ) : (
        <Button aria-label="Accept and continue" type="submit">
          Accept and continue
        </Button>
      )}
    </form>
  );
};
