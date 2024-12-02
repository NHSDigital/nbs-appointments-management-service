import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { Button, InsetText } from '@components/nhsuk-frontend';
import { acceptEula, fetchEula } from '@services/appointmentsService';
import Link from 'next/link';
import { notFound } from 'next/navigation';

export type EulaPageProps = {
  searchParams?: {
    user?: string;
  };
};

const Page = async ({ searchParams }: EulaPageProps) => {
  const latestVersion = await fetchEula();

  if (searchParams?.user === undefined) {
    notFound();
  }

  return (
    <NhsAnonymousPage title="Agree to the End User Licence Agreement">
      <InsetText>
        You must read the End-User Licence Agreement before using this
        application.
      </InsetText>
      <p>
        <Link href={'/eula/full-terms'}>
          Read the full user agreement set out in the end-user licence agreement
        </Link>
      </p>

      <p>
        By continuing, you agree to the full{' '}
        <Link href="/terms-of-use">terms of use</Link> and{' '}
        <Link href="/privacy-policy">privacy policy</Link>.
      </p>

      <form
        action={acceptEula.bind(
          null,
          searchParams.user,
          latestVersion.versionDate,
        )}
      >
        <Button aria-label="Accept and continue" type="submit">
          Accept and continue
        </Button>
      </form>
    </NhsAnonymousPage>
  );
};

export default Page;
