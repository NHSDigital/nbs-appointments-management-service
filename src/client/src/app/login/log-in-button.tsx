import redirectToIdServer from '../auth/redirectToIdServer';
import { Button } from '@nhsuk-frontend-components';
import Link from 'next/link';

type LogInButtonProps = {
  redirectUrl: string;
  provider: string;
  friendlyName: string;
  styleType?: ButtonStyleType;
};

type ButtonStyleType = 'primary' | 'secondary';

const LogInButton = ({
  redirectUrl,
  provider,
  friendlyName,
  styleType = 'primary',
}: LogInButtonProps) => {
  return (
    <>
      {styleType === 'primary' && (
        <form action={redirectToIdServer.bind(null, redirectUrl, provider)}>
          <Button
            aria-label={`Sign in to service with ${friendlyName}`}
            type="submit"
            styleType="primary"
          >
            Sign in to service with {friendlyName}
          </Button>
        </form>
      )}
      {styleType === 'secondary' && (
        <Link
          href={`auth/login?provider=${provider}&redirectUrl=${redirectUrl}`}
          aria-label={`Sign in to service with ${friendlyName}`}
        >
          Sign in to service with {friendlyName}
        </Link>
      )}
    </>
  );
};

export default LogInButton;
