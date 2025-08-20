import ContactUs from '@components/contact-us';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import RefreshButton from '@components/refresh-button';
import Link from 'next/link';

// TODO: Update this page with approved copy
export default function NotFound() {
  return (
    <NhsAnonymousPage
      title="Sorry, we could not find that page"
      showHomeBreadcrumb
      originPage="not-found"
    >
      <p>
        You may have typed or pasted the web address incorrectly.{' '}
        <Link href="/sites">Go to the start page.</Link>
      </p>

      <p>
        There is a known issue with signing in. If this happens, please try
        refreshing the page and you will see the login screen.
      </p>

      <RefreshButton />

      <ContactUs />
    </NhsAnonymousPage>
  );
}
