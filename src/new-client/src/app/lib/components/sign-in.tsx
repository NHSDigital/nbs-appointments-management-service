'use client';
import { useSearchParams, usePathname } from 'next/navigation';
import React, { useTransition } from 'react';
import { When } from '@components/when';
import { authenticationEndpoint, fetchAccessToken } from '@services/nbsService';

export const SignIn = () => {
  const searchParams = useSearchParams();
  const hasCode = searchParams.has('code');
  const code = searchParams.get('code') ?? '';
  const pathname = usePathname();
  const [, startTransition] = useTransition();
  const [returnUrl, setReturnUrl] = React.useState('');

  React.useEffect(() => {
    if (hasCode) {
      startTransition(async () => {
        await fetchAccessToken(code);
      });
    }
    setReturnUrl(window.location.href);
  }, [hasCode, pathname, code, searchParams]);

  return (
    <When condition={!hasCode}>
      <div style={{ margin: '20px' }}>
        <a
          className="nhsuk-button"
          href={`${authenticationEndpoint}?redirect_uri=${returnUrl}`}
        >
          Sign In
        </a>
      </div>
    </When>
  );
};
