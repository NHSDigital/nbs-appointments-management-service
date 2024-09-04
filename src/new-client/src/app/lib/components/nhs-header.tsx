import { When } from '@components/when';
import NhsHeaderLogIn from '@components/nhs-header-log-in';
import NhsHeaderLogOut from '@components/nhs-header-log-out';
import { Header } from '@nhsuk-frontend-components';

type NhsHeaderProps = {
  userEmail?: string;
  // We still show the header in the error handling page, which has no access to the userProfile call
  // So to avoid it showing "Log in" when you're already logged in, we need to pass this prop for now
  // This is a temporary solution until we have a better way to handle this
  showAuthControls?: boolean;
};

export const NhsHeader = ({
  userEmail,
  showAuthControls = true,
}: NhsHeaderProps) => {
  return (
    <Header>
      <When condition={showAuthControls}>
        <When condition={userEmail !== undefined}>
          <span className="nhsuk-header-custom__user-control">{userEmail}</span>
          <NhsHeaderLogOut />
        </When>
        <When condition={userEmail === undefined}>
          <NhsHeaderLogIn />
        </When>
      </When>
    </Header>
  );
};
