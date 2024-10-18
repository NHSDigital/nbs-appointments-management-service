'use client';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import Link from 'next/link';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  if (error.message === 'Forbidden: You lack the necessary permissions') {
    <NhsAnonymousPage title="You cannot access this page">
      <p>
        This might be because:
        <ol>
          <li>you don't have an account to access this service</li>
          <li>you don't have permissions to access this page</li>
        </ol>
        If you think this is wrong, contact us to let us know:
      </p>

      <p>
        <strong>By email</strong> <br />
        <Link href="enquiries@nhsdigital.nhs.uk">
          enquiries@nhsdigital.nhs.uk
        </Link>
      </p>

      <p>
        <strong>By phone</strong>
        <br />
        0300 303 5678 <br />
        Monday to friday <br />
        9am to 5pm excluding bank holidays
      </p>
    </NhsAnonymousPage>;
  }

  // TODO: write error.digest to application insights?
  // console.dir(error.digest);

  return (
    <NhsAnonymousPage title="Sorry, there is a problem with this service">
      <p>
        Please try again later or contact us to let us know there is a problem.
      </p>

      <p>
        <strong>By email</strong> <br />
        <Link href="enquiries@nhsdigital.nhs.uk">
          enquiries@nhsdigital.nhs.uk
        </Link>
      </p>

      <p>
        <strong>By phone</strong>
        <br />
        0300 303 5678 <br />
        Monday to friday <br />
        9am to 5pm excluding bank holidays
      </p>
    </NhsAnonymousPage>
  );
}
