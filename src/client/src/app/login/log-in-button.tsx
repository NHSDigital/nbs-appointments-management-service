import redirectToIdServer from '../auth/redirectToIdServer';
import { Button } from '@nhsuk-frontend-components';

type LogInButtonProps = {
  redirectUrl: string;
};

const LogInButton = ({ redirectUrl }: LogInButtonProps) => {
  return (
    <form action={redirectToIdServer.bind(null, redirectUrl)}>
      <Button aria-label="Sign in to service" type="submit">
        Sign in to service
      </Button>
    </form>
  );
};

export default LogInButton;
