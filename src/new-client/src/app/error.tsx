'use client';
import NhsFooter from '@components/nhs-footer';
import { NhsHeader } from '@components/nhs-header';
import NhsMainContainer from '@components/nhs-main-container';
import { Breadcrumbs } from '@components/nhsuk-frontend';
import Link from 'next/link';

export default function Error({
  error,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  if (error.message === 'Forbidden: You lack the necessary permissions') {
    <>
      <NhsHeader showAuthControls={false} />
      <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      <NhsMainContainer>
        <h1 className="app-page-heading">You cannot access this page</h1>
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
      </NhsMainContainer>
      <NhsFooter />
    </>;
  }

  // TODO: write error.digest to application insights?
  // console.dir(error.digest);

  return (
    <>
      <NhsHeader showAuthControls={false} />
      <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      <NhsMainContainer>
        <h1 className="app-page-heading">
          Sorry, there is a problem with this service
        </h1>
        <p>
          Please try again later or contact us to let us know there is a
          problem.
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
      </NhsMainContainer>
      <NhsFooter />
    </>
  );
}
