'use client';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { saveUserRoleAssignments } from '@services/appointmentsService';
import { Button, ButtonGroup } from '@nhsuk-frontend-components';
import { SummaryList } from '@components/nhsuk-frontend';
import {
  SummaryListItem,
  LinkActionProps,
  OnClickActionProps,
} from '@components/nhsuk-frontend';

const UserSummary = () => {
  const { push } = useRouter();
  const [formData, setFormData] = useState<UserDetails | null>(null);

  type UserDetails = {
    site: string;
    user: string;
    firstName: string;
    lastName: string;
    roles: string[];
    isEdit: boolean;
  };

  useEffect(() => {
    // Retrieve stored data
    const storedData = sessionStorage.getItem('userFormData');
    if (storedData) {
      setFormData(JSON.parse(storedData));
    } else {
      push('/'); // Redirect if no data is found
    }
  }, [push]);

  const handleSubmit = async () => {
    if (!formData) return;

    await saveUserRoleAssignments(
      formData.site,
      formData.user,
      formData.firstName,
      formData.lastName,
      formData.roles,
      formData.isEdit,
    );

    push(`/site/${formData.site}/users`);
  };

  const userDetailsPagePath = `/site/${formData?.site}/users/manage?user=${formData?.user}`;
  const addUserPagePath = `/site/${formData?.site}/users/manage`;
  const isNhsUser = formData?.user?.endsWith('nhs.net') ?? false;
  const changeAction = (
    href: string,
    hideButton: boolean,
  ): LinkActionProps | OnClickActionProps | undefined => {
    if (hideButton) {
      return undefined;
    }

    return {
      renderingStrategy: 'server',
      text: 'Change',
      href: href,
    };
  };

  const userDetails: SummaryListItem[] = [
    {
      title: 'Name',
      value: isNhsUser ? '' : `${formData?.firstName} ${formData?.lastName}`,
      action: changeAction(userDetailsPagePath, isNhsUser),
    },
    {
      title: 'Email address',
      value: formData?.user,
      action: changeAction(addUserPagePath, !!formData?.isEdit),
    },
    {
      title: 'Roles',
      value: formData?.roles.join(', '),
      action: changeAction(userDetailsPagePath, false),
    },
  ];

  const getSubmissionNote = () => {
    if (formData?.isEdit) {
      return '';
    }

    if (isNhsUser) {
      return `${formData?.user} will be sent information about how to login.`;
    }

    return `${formData?.firstName} will be sent information about how to login.`;
  };

  if (!formData) return <p>Loading...</p>;

  return (
    <div>
      {userDetails && <SummaryList items={userDetails}></SummaryList>}

      <p aria-label="submition-note">{getSubmissionNote()}</p>

      <ButtonGroup>
        <Button onClick={handleSubmit}>Confirm and send</Button>
      </ButtonGroup>
    </div>
  );
};

export default UserSummary;
