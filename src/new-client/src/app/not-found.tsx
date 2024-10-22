import NhsFooter from '@components/nhs-footer';
import { NhsHeader } from '@components/nhs-header';
import NhsMainContainer from '@components/nhs-main-container';
import { Breadcrumbs } from '@components/nhsuk-frontend';
import Link from 'next/link';

// TODO: Update this page with approved copy
export default async function NotFound() {
  return (
    <>
      <NhsHeader showAuthControls={false} />
      <Breadcrumbs trail={[{ name: 'Home', href: '/' }]} />
      <NhsMainContainer>
        <h1 className="app-page-heading">Sorry, we could not find that page</h1>
        <p>
          You may have typed or pasted the web address incorrectly.{' '}
          <Link href="/">Go to the start page.</Link>
        </p>

        <p>
          If the web address is correct or you selected a link or button,
          contact us to let us know there is a problem with this page:
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
