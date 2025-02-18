'use client';
import { Button } from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';

type LogInButtonProps = {
  authHost: string;
  provider: string;
  friendlyName: string;
};

const LogInButton = ({
  authHost,
  provider,
  friendlyName,
}: LogInButtonProps) => {
  const router = useRouter();

  const idServerEndpoint = `${authHost}/api/authenticate?provider=${provider}`;

  return (
    <Button
      aria-label={`Sign in to service with ${friendlyName}`}
      type="submit"
      onClick={() => {
        router.push(idServerEndpoint);
      }}
    >
      Sign in to service with {friendlyName}
    </Button>
  );
};

export default LogInButton;
