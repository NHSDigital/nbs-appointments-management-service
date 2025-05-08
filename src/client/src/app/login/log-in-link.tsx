import Link from 'next/link';

type LogInButtonProps = {
  redirectUrl: string;
  provider: string;
  friendlyName: string;
};

const LogInLink = ({
  redirectUrl,
  provider,
  friendlyName,
}: LogInButtonProps) => {
  return (
    <Link
      href={`auth/login/${provider}?redirectUrl=${redirectUrl}`}
      aria-label={`Sign in to service with ${friendlyName}`}
    >
      Sign in to service with {friendlyName}
    </Link>
  );
};

export default LogInLink;
