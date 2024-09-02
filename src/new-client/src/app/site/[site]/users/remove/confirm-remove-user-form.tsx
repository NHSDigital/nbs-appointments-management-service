/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';
import { removeUserFromSite } from '@services/appointmentsService';
import { Button, ButtonGroup } from '@nhsuk-frontend-components';

const ConfirmRemoveUserForm = ({
  site,
  user,
}: {
  site: string;
  user: string;
}) => {
  const { replace } = useRouter();
  const { handleSubmit } = useForm();

  const cancel = () => {
    replace(`/site/${site}/users`);
  };

  const submitForm: SubmitHandler<object> = async () => {
    await removeUserFromSite(site, user);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <ButtonGroup>
        <Button type="submit" styleType="warning">
          Yes, remove this account
        </Button>
        <Button styleType="secondary" onClick={cancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default ConfirmRemoveUserForm;
