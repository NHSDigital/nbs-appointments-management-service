import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { Button, InsetText } from '@components/nhsuk-frontend';
import { acceptEula, fetchEula } from '@services/appointmentsService';

const Page = async () => {
  const latestVersion = await fetchEula();

  return (
    <NhsAnonymousPage title="Agree to the End User Licence Agreement">
      <InsetText>
        You must read the End-User Licence Agreement before using this
        application.
      </InsetText>

      {/* // TODO: Add these links back in once we have appropriate content
      <p>
        <Link href={'/eula/full-terms'}>
          Read the full user agreement set out in the end-user licence agreement
        </Link>
      </p>

      <p>
        By continuing, you agree to the full{' '}
        <Link href="/terms-of-use">terms of use</Link> and{' '}
        <Link href="/privacy-policy">privacy policy</Link>.
      </p> */}

      <form action={acceptEula.bind(null, latestVersion.versionDate)}>
        <Button aria-label="Accept and continue" type="submit">
          Accept and continue
        </Button>
      </form>
    </NhsAnonymousPage>
  );
};

export default Page;
