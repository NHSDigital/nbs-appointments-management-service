import { When } from '@components/when';
import NhsHeaderLogIn from '@components/nhs-header-log-in';
import NhsHeaderLogOut from '@components/nhs-header-log-out';
import { Header } from '@nhsuk-frontend-components';
import { getSession } from '../../auth/getSession';

type NhsHeaderProps = {
  showAuthControls?: boolean;
};

export const NhsHeader = ({ showAuthControls = true }: NhsHeaderProps) => {
  const session = getSession();

  return (
    <Header>
      <When condition={showAuthControls}>
        <When condition={session?.emailAddress !== undefined}>
          <span className="nhsuk-header-custom__user-control">
            {session?.emailAddress}
          </span>
          <NhsHeaderLogOut />
        </When>
        <When condition={session?.emailAddress === undefined}>
          <NhsHeaderLogIn />
        </When>
      </When>
    </Header>
  );
};
