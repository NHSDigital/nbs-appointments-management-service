import ContactUs from '@components/contact-us';
import NhsAnonymousPage from '@components/nhs-anonymous-page';
import Link from 'next/link';

// TODO: Update this page with approved copy
export default async function NotFound() {
  return (
    <NhsAnonymousPage
      title="Sorry, we could not find that page"
      showHomeBreadcrumb
    >
      <p>
        You may have typed or pasted the web address incorrectly.{' '}
        <Link href="/">Go to the start page.</Link>
      </p>

      <ContactUs
        prompt={
          'If the web address is correct or you selected a link or button, contact us to let us know there is a problem with this page:'
        }
      />
    </NhsAnonymousPage>
  );
}
