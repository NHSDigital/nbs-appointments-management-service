import Link from 'next/link';

// TODO: Update this component with real contact details.
// Unfortunately, because these appear on the error page
// which is a server component we can't store them in cosmos,
// but they could be stored as constants perhaps.
const ContactUs = ({ prompt = 'Contact us:' }: { prompt: string }) => {
  return (
    <>
      <p>{prompt}</p>

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
    </>
  );
};

export default ContactUs;
