import redirectToIdServer from '../auth/redirectToIdServer';
import { Button } from '@nhsuk-frontend-components';

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
    <form action={redirectToIdServer.bind(null, redirectUrl, provider)}>
      <Button
        aria-label={`Sign in to service with ${friendlyName}`}
        type="submit"
        styleType={styleType}
      >
        Sign in to service with {friendlyName}
      </Button>
    </form>
  );
};

export default LogInButton;
