'use client';
import ContactUs from '@components/contact-us';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { ErrorType, MyaError } from '@types';

export default function Error({
  error,
}: {
  error: MyaError;
  reset: () => void;
}) {
  if ((error.digest as ErrorType) === 'UnauthorizedError') {
    return (
      <NhsAnonymousPage
        title="You cannot access this page"
        showHomeBreadcrumb
        originPage="error"
      >
        <p>This might be because: </p>
        <ul>
          <li>you don't have an account to access this service</li>
          <li>you don't have permissions to access this page</li>
        </ul>

        <ContactUs />
      </NhsAnonymousPage>
    );
  }

  return (
    <NhsAnonymousPage
      title="Sorry, there is a problem with this service"
      showHomeBreadcrumb
      originPage="error"
    >
      <ContactUs />
    </NhsAnonymousPage>
  );
}
