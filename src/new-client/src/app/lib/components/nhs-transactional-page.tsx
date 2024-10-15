'use client';
import { ReactNode } from 'react';
import { NhsHeader } from './nhs-header';
import { UserProfile } from '@types';

type Props = {
  children: ReactNode;
  userProfile: UserProfile;
};

const NhsTransactionalPage = ({ children = null, userProfile }: Props) => {
  return (
    <>
      <NhsHeader userEmail={userProfile.emailAddress} />
      <div className="nhsuk-width-container">
        <main className="nhsuk-main-wrapper" id="main-content" role="main">
          <div className="nhsuk-grid-row">
            <div className="nhsuk-grid-column-full">{children}</div>
          </div>
        </main>
      </div>
    </>
  );
};

export default NhsTransactionalPage;
