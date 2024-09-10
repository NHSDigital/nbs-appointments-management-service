/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useRouter } from 'next/navigation';
import { removeUserFromSite } from '@services/appointmentsService';
import { Button, ButtonGroup } from '@nhsuk-frontend-components';
import { Site } from '@types';

const RemoveUserPage = ({ site, user }: { site: Site; user: string }) => {
  const { replace } = useRouter();
  const { handleSubmit } = useForm();

  const cancel = () => {
    replace(`/site/${site.id}/users`);
  };

  const submitForm: SubmitHandler<object> = async () => {
    await removeUserFromSite(site.id, user);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <p>
        Are you sure you wish to remove {user} from {site.name}?
      </p>
      <p>This will:</p>
      <ul>
        <li>Revoke the user's access to this site only</li>
        <li>
          This will not affect their wider @nhs.net account - this will only
          remove them from the National Booking System
        </li>
        <li>This will not remove their access from any other sites</li>
      </ul>

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

export default RemoveUserPage;
