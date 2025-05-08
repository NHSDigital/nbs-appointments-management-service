import redirectToIdServer from '../auth/redirectToIdServer';
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
    <form action={redirectToIdServer.bind(null, redirectUrl, provider)}>
      <Button
        aria-label={`Sign in to service with ${friendlyName}`}
        type="submit"
        styleType="primary"
      >
        Sign in to service with {friendlyName}
      </Button>
    </form>
  );
};

export default LogInButton;
