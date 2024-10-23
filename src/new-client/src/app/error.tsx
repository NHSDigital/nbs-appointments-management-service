'use client';
import ContactUs from '@components/contact-us';
import NhsAnonymousPage from '@components/nhs-anonymous-page';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  // TODO: write error.digest to application insights?
  // console.dir(error.digest);

  if (error.message === 'Forbidden: You lack the necessary permissions') {
    <NhsAnonymousPage title="You cannot access this page" showHomeBreadcrumb>
      <p>
        This might be because:
        <ol>
          <li>you don't have an account to access this service</li>
          <li>you don't have permissions to access this page</li>
        </ol>
      </p>

      <ContactUs
        prompt={'If you think this is wrong, contact us to let us know:'}
      />
    </NhsAnonymousPage>;
  }

  return (
    <NhsAnonymousPage
      title="Sorry, there is a problem with this service"
      showHomeBreadcrumb
    >
      <ContactUs
        prompt={
          'Please try again later or contact us to let us know there is a problem:'
        }
      />
    </NhsAnonymousPage>
  );
}
