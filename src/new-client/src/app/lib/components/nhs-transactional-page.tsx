'use client';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import { UserProfile } from '@types';
import NhsMainContainer from './nhs-main-container';

type Props = {
  children: ReactNode;
  userProfile: UserProfile;
};

const NhsTransactionalPage = ({ children = null, userProfile }: Props) => {
  return (
    <>
      <NhsHeader userEmail={userProfile.emailAddress} />
      <NhsMainContainer>{children}</NhsMainContainer>
    </>
  );
};

export default NhsTransactionalPage;
