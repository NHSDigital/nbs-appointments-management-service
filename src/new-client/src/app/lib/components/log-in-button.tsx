import redirectToIdServer from '../../auth/redirectToIdServer';
import { Button } from '@nhsuk-frontend-components';

const LogInButton = () => {
  return (
    <form action={redirectToIdServer.bind(null, undefined)}>
      <Button aria-label="Sign in to service" type="submit">
        Sign in to service
      </Button>
    </form>
  );
};

export default LogInButton;
