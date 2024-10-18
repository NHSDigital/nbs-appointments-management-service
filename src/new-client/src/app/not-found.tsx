import NhsAnonymousPage from '@components/nhs-anonymous-page';
import Link from 'next/link';

// TODO: Update this page with approved copy
export default async function NotFound() {
  return (
    <NhsAnonymousPage title="Sorry, we could not find that page">
      <p>
        You may have typed or pasted the web address incorrectly.{' '}
        <Link href="/">Go to the start page.</Link>
      </p>

      <p>
        If the web address is correct or you selected a link or button, contact
        us to let us know there is a problem with this page:
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
