import Link from 'next/link';
import { Button } from '@nhsuk-frontend-components';

type LogInButtonProps = {
  redirectUrl: string;
  provider: string;
  friendlyName: string;
};

const LogInButton = ({
  redirectUrl,
  provider,
  friendlyName,
}: LogInButtonProps) => {
  return (
    <Link
      href={`auth/login/${provider}?redirectUrl=${redirectUrl}`}
      aria-label={`Sign in to service with ${friendlyName}`}
      prefetch={false}
    >
      <Button
        aria-label={`Sign in to service with ${friendlyName}`}
        type="submit"
        styleType="primary"
      >
        Sign in to service with {friendlyName}
      </Button>
    </Link>
  );
};

export default LogInButton;
